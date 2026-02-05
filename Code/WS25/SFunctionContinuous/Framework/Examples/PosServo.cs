using SFunctionContinuous.Framework.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFunctionContinuous.Framework.Examples
{
    public class PosServo : Example
    {
        public PosServo()
        {
            Block constant = new ConstantBlock("Constant", 1);
            Block gain1 = new GainBlock("Gain_1", 0.1);
            Block gain2 = new GainBlock("Gain_2", 1);
            Block sub = new SubtractBlock("Sub");
            Block integrate1 = new IntegrateBlock("Integrate1", 0);
            Block integrate2 = new IntegrateBlock("Integrate2", 0);
            
            Block recordX = new RecordBlock("Strecke");
            Block recordV = new RecordBlock("Geschwindigkeit");
            Block recordA = new RecordBlock("Becshleunigung");
            
            Model.AddBlock(constant);
            Model.AddBlock(gain1);
            Model.AddBlock(gain2);
            Model.AddBlock(sub);
            Model.AddBlock(integrate1);
            Model.AddBlock(integrate2);
            Model.AddBlock(recordX);
            Model.AddBlock(recordV);
            Model.AddBlock(recordA);
            
            Model.AddConnection(constant, 0, gain1, 0);
            Model.AddConnection(gain1, 0, sub, 0);
            Model.AddConnection(sub, 0, integrate1, 0);
            Model.AddConnection(integrate1, 0, integrate2, 0);
            
            Model.AddConnection(integrate1, 0, gain2, 0);
            Model.AddConnection(gain2, 0, sub, 1);

            Model.AddConnection(sub, 0, recordA, 0);
            Model.AddConnection(integrate1, 0, recordV, 0);
            Model.AddConnection(integrate2, 0, recordX, 0);
        }
    }
}
