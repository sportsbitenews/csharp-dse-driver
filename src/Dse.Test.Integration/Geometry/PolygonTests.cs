﻿//
//  Copyright (C) 2016 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dse.Test.Integration.TestClusterManagement;
using Dse.Geometry;

namespace Dse.Test.Integration.Geometry
{
    public class PolygonTests : GeometryTests<Polygon>
    {
        protected override Polygon[] Values
        {
            get
            {
                return new[]
                {
                    new Polygon(new Point(1, 3), new Point(3, -11.2), new Point(3, 6.2), new Point(1, 3)),
                    new Polygon(
                        new[] {new Point(-10, 10), new Point(10, 0), new Point(10, 10), new Point(-10, 10)},
                        new[] {new Point(6, 7), new Point(3, 9), new Point(9, 9), new Point(6, 7)}),
                    new Polygon()
                };
            }
        }

        protected override string TypeName
        {
            get { return "PolygonType"; }
        }
    }
}
