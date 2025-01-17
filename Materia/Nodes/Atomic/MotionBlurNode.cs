﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Nodes.Attributes;
using Materia.Textures;
using OpenTK;

namespace Materia.Nodes.Atomic
{
    public class MotionBlurNode : ImageNode
    {
        MotionBlurProcessor processor;

        int magnitude;
        [Slider(IsInt = true, Max = 128, Min = 1, Snap = false, Ticks = new float[0])]
        public int Intensity
        {
            get
            {
                return magnitude;
            }
            set
            {
                magnitude = value;
                TryAndProcess();
            }
        }

        int direction;
        [Slider(IsInt = true, Max = 180, Min = 0, Snap = false, Ticks = new float[0])]
        public int Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                TryAndProcess();
            }
        }

        NodeInput input;
        NodeOutput output;

        public MotionBlurNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Motion Blur";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();
            processor = new MotionBlurProcessor();

            tileX = tileY = 1;

            direction = 0;
            magnitude = 10;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            output.Data = null;
            output.Changed();
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
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

            CreateBufferIfNeeded();

            processor.TileX = 1;
            processor.TileY = 1;
            processor.Direction = (float)direction * (float)(Math.PI / 180.0f);
            processor.Magnitude = magnitude;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;
            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();
            previewProcessor.TileX = 1;
            previewProcessor.TileY = 1;

            Updated();
            output.Data = buffer;
            output.Changed();
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {

        }

        public override string GetJson()
        {
            return "";
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Release();
            }
        }
    }
}
