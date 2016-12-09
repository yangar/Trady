﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trady.Strategy.Rule
{
    public abstract class BinaryOperationBase<T> : OperationBase<T>
    {
        protected BinaryOperationBase(IRule<T> operand1, IRule<T> operand2) 
            : base(operand1 ?? throw new ArgumentNullException(nameof(operand1)), operand2 ?? throw new ArgumentNullException(nameof(operand2)))
        {
        }

        protected IRule<T> Operand1 => Operands.ElementAt(0);

        protected IRule<T> Operand2 => Operands.ElementAt(1);

        public override IRule<T> Operate(T obj, int index)
        {
            var operand1Value = Operand1.IsValid(obj, index);
            var result = Operate(operand1Value) ?? Operate(operand1Value, Operand2.IsValid(obj, index));
            if (!result.HasValue)
                throw new Exception("'Operate' method may be mis-implemented, please make sure it does not return null if two operands are input");
            return new Rule<T>(result.Value);
        }

        protected abstract bool? Operate(bool operand1Value, bool? operand2Value = null);
    }
}