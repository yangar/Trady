﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trady.Strategy.Rule
{
    public abstract class UnaryOperationBase<T> : OperationBase<T>
    {
        protected UnaryOperationBase(IRule<T> operand) 
            : base(operand ?? throw new ArgumentNullException(nameof(operand)))
        {
        }

        protected IRule<T> Operand => Operands.ElementAt(0);

        public override IRule<T> Operate(T obj)
            => new Rule<T>(Operate(Operand.IsValid(obj)));

        protected abstract bool Operate(bool operandValue);
    }
}
