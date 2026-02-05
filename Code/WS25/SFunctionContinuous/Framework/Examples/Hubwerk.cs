using SFunctionContinuous.Framework.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFunctionContinuous.Framework.Examples
{
    public class Hubwerk : Example
    {
        public Hubwerk()
        {
            double _r2 = 2;
            double _r32 = 5;
            double _r34 = 2;
            double _r43 = 4;
            double _r45 = 2;
            double m = 10;
            double g = 9.81;
            double jRed = 1.6;
            double M2 = 39;
            double a = (_r2 * _r34 * _r45) / (_r32 * _r43);
            
            Block constantM = new ConstantBlock("Constant", (M2/jRed));
            Block constantC = new ConstantBlock("Constant", ((m*g*a)/jRed));
            Block sub = new SubtractBlock("Sub");
            Block integrate1 = new IntegrateBlock("Integrate1", 0);
            Block integrate2 = new IntegrateBlock("Integrate2", 0);
            Block recordphi = new RecordBlock("Phi");
            Block recordphidot = new RecordBlock("Winkelgeschwindigkeit");
            Block recordphidotdot = new RecordBlock("Winkelbeschleunigung");
            
            Model.AddBlock(constantM);
            Model.AddBlock(constantC);
            Model.AddBlock(sub);
            Model.AddBlock(integrate1);
            Model.AddBlock(integrate2);
            Model.AddBlock(recordphi);
            Model.AddBlock(recordphidot);
            Model.AddBlock(recordphidotdot);
            
            Model.AddConnection(constantM, 0, sub, 0);
            Model.AddConnection(constantC, 0, sub, 1);
            Model.AddConnection(sub, 0, integrate1, 0);
            Model.AddConnection(integrate1, 0, integrate2, 0);
            Model.AddConnection(integrate2, 0, recordphi, 0);            
            Model.AddConnection(integrate1, 0, recordphidot, 0);
            Model.AddConnection(sub, 0, recordphidotdot, 0);
        }
    }
}
