﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Materia.Nodes.Atomic
{
    public class TransformNode : ImageNode
    {
        TransformProcessor processor;

        protected float xoffset;

        [Title(Title = "Offset X")]
        public float XOffset
        {
            get
            {
                return xoffset;
            }
            set
            {
                xoffset = value;
                TryAndProcess();
            }
        }

        protected float yoffset;

        [Title(Title = "Offset Y")]
        public float YOffset
        {
            get
            {
                return yoffset;
            }
            set
            {
                yoffset = value;
                TryAndProcess();
            }
        }

        protected float angle;

        [Slider(IsInt = false, Max = 360, Min = 0, Snap = false, Ticks = new float[0])]
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                TryAndProcess();
            }
        }

        protected float scaleX;
        [Title(Title = "Scale X")]
        public float ScaleX
        {
            get
            {
                return scaleX;
            }
            set
            {
                scaleX = value;
                TryAndProcess();
            }
        }
        protected float scaleY;
        [Title(Title = "Scale Y")]
        public float ScaleY
        {
            get
            {
                return scaleY;
            }
            set
            {
                scaleY = value;
                TryAndProcess();
            }
        }

        NodeOutput Output;
        NodeInput input;

        public TransformNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Transform";

            Id = Guid.NewGuid().ToString();

            angle = xoffset = yoffset = 0;
            scaleX = scaleY = 1;

            tileX = tileY = 1;

            width = w;
            height = h;

            previewProcessor = new BasicImageRenderer();
            processor = new TransformProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(input.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            Matrix3 rot = Matrix3.CreateRotationZ(angle * (float)(Math.PI / 180.0));
            Matrix3 scale = Matrix3.CreateScale(1.0f / scaleX, 1.0f / scaleY, 1);
            Vector3 trans = new Vector3(xoffset, yoffset, 0);

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Rotation = rot;
            processor.Scale = scale;
            processor.Translation = trans;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public override void Dispose()
        {
            base.Dispose();

            //we always release just in case
            //we ever add anything to it
            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }

        public class TransformData : NodeData
        {
            public float xOffset;
            public float yOffset;
            public float angle;
            public float scaleX;
            public float scaleY;
        }

        public override string GetJson()
        {
            TransformData d = new TransformData();
            FillBaseNodeData(d);
            d.xOffset = xoffset;
            d.yOffset = yoffset;
            d.angle = angle;
            d.scaleX = scaleX;
            d.scaleY = scaleY;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            TransformData d = JsonConvert.DeserializeObject<TransformData>(data);

            SetBaseNodeDate(d);

            xoffset = d.xOffset;
            yoffset = d.yOffset;
            angle = d.angle;
            scaleX = d.scaleX;
            scaleY = d.scaleY;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }
    }
}
