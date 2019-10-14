using System.Collections.Generic;

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
namespace Neo4Net.Tooling.procedure.testutils
{

	using JavaFileObjects = com.google.testing.compile.JavaFileObjects;

	public sealed class JavaFileObjectUtils
	{
		 public static readonly JavaFileObjectUtils Instance = new JavaFileObjectUtils( "Instance", InnerEnum.Instance );

		 private static readonly IList<JavaFileObjectUtils> valueList = new List<JavaFileObjectUtils>();

		 static JavaFileObjectUtils()
		 {
			 valueList.Add( Instance );
		 }

		 public enum InnerEnum
		 {
			 Instance
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private JavaFileObjectUtils( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public javax.tools.JavaFileObject ProcedureSource( string relativePath )
		 {
			  return JavaFileObjects.forResource( ResolveUrl( relativePath ) );
		 }

		 private java.net.URL ResolveUrl( string relativePath )
		 {
			  return this.GetType().getResource("/org/neo4j/tooling/procedure/procedures/" + relativePath);
		 }

		public static IList<JavaFileObjectUtils> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static JavaFileObjectUtils valueOf( string name )
		{
			foreach ( JavaFileObjectUtils enumInstance in JavaFileObjectUtils.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}