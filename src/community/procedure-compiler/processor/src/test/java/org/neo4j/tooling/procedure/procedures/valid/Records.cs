﻿using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Tooling.procedure.procedures.valid
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;

	public class Records
	{

		 public class LongWrapper
		 {
			  public readonly long? Value;

			  public LongWrapper( long? value )
			  {
					this.Value = value;
			  }
		 }

		 public class SimpleTypesWrapper
		 {
			  public string Field01 = "string";
			  public long Field02 = 2;
			  public long? Field03 = 3L;
			  public Number Field04 = 4.0;
			  public bool? Field05 = true;
			  public bool Field06 = true;
			  public object Field07;
			  public Node Field08;
			  public Path Field09;
			  public Relationship Field10;
		 }

		 public class GenericTypesWrapper
		 {
			  public IList<string> Field01;
			  public IList<long> Field03;
			  public IList<Number> Field04;
			  public IList<bool> Field05;
			  public IList<object> Field07;
			  public IList<Node> Field08;
			  public IList<Path> Field09;
			  public IList<Relationship> Field10;
			  public IDictionary<string, string> Field11;
			  public IDictionary<string, long> Field13;
			  public IDictionary<string, Number> Field14;
			  public IDictionary<string, bool> Field15;
			  public IDictionary<string, object> Field17;
			  public IDictionary<string, Node> Field18;
			  public IDictionary<string, Path> Field19;
			  public IDictionary<string, Relationship> Field20;
			  public IList<IList<Relationship>> Field21;
			  public IList<IDictionary<string, Relationship>> Field22;
		 }
	}

}