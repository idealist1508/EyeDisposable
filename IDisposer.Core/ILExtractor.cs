﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace IDisposer.Core
{
    static class ILExtractor
    {
        const string kIDisposableFullName = "System.IDisposable";
        const string kTypeVoidFullName = "System.Void";

        public class Result
        {
            public readonly List<Instruction> NewObjs;
            public readonly List<Instruction> Disposes;

            public Result(List<Instruction> newobjs, List<Instruction> disposes)
            {
                Disposes = disposes;
                NewObjs = newobjs;
            }

            public bool IsEmpty
            {
                get
                {
                    return Disposes.Count == 0 && NewObjs.Count == 0;
                }
            }
        }

        public static bool ContainsCallTo(MethodBody m,
            string methodFullName)
        {
            return m.Instructions.Any(i =>
                (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt) &&
                ((MethodReference)i.Operand).FullName == methodFullName);
        }

        public static Result FindNewObjsAndDisposes(MethodBody m)
        {
            List<Instruction> newobjs = new List<Instruction>();
            List<Instruction> disposes = new List<Instruction>();

            foreach (var i in m.Instructions)
            {
                var operandMethod = i.Operand as MethodReference;
                if (operandMethod == null)
                    continue;

                if (i.OpCode == OpCodes.Newobj &&
                    operandMethod.DeclaringType.HasInterface(
                    kIDisposableFullName))
                {
                    newobjs.Add(i);
                }

                else if (i.OpCode == OpCodes.Callvirt ||
                    i.OpCode == OpCodes.Call)
                {
                    //if (operandMethod.FullName == drAdd.FullName)
                    //    throw new InvalidOperationException(
                    //        "Assembly seems already instrumented.");

                    if(
                        operandMethod.Name == "Dispose" &&
                        operandMethod.Parameters.Count == 0 &&
                        operandMethod.ReturnType.Resolve().FullName ==
                            kTypeVoidFullName)
                    {
                        // We don't do value types yet
                        if(operandMethod.DeclaringType.IsValueType)
                            continue;

                        // This may happen if the user code contains 
                        // a `using` block with a value type 
                        // implementing IDisposable
                        else if (i.Previous.OpCode == OpCodes.Constrained)
                        {
                            var constrainedType =
                                i.Previous.Operand as TypeDefinition;

                            if(constrainedType != null &&
                                constrainedType.IsValueType)
                                continue;
                        }

                        disposes.Add(i);
                    }
                }
            }

            return new Result(newobjs, disposes);
        }
    }
}
