//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Linq.Expressions;

namespace Dse.Data.Linq
{
    public class CqlLinqNotSupportedException : NotSupportedException
    {
        public Expression Expression { get; private set; }

        internal CqlLinqNotSupportedException(Expression expression, ParsePhase parsePhase)
            : base(string.Format("The expression {0} = [{1}] is not supported in {2} parse phase.",
                                 expression.NodeType, expression, parsePhase))
        {
            Expression = expression;
        }
    }
}