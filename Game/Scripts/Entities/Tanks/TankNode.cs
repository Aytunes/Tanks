using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CryEngine;
using CryEngine.FlowSystem;

namespace CryGameCode.Entities
{
    public class MyEntityNode : EntityFlowNode<MyEntity>
    {
        [Port]
        public void Vec3Test(Vec3 input) { }

        [Port]
        public void FloatTest(float input) { }

        [Port]
        public void VoidTest()
        {

        }

        [Port]
        OutputPort<bool> BoolOutput { get; set; }
    }

    [FlowNode(Category = "MyCategory", Filter = FlowNodeFilter.Approved)]
    public class TestNode : FlowNode
    {
        [Port]
        public void InputTestVoid()
        {
        }

        [Port]
        public void InputTestVec3(Vec3 input)
        {
        }

        [Port]
        OutputPort<float> FloatOutput { get; set; }
    }
}
