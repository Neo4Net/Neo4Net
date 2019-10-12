using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Cypher.@internal.codegen
{

	using Node = Neo4Net.Graphdb.Node;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public class PrimitiveNodeStream : PrimitiveEntityStream<VirtualNodeValue>
	{
		 public PrimitiveNodeStream( LongStream inner ) : base( inner )
		 {
		 }

		 public static PrimitiveNodeStream Of( long[] array )
		 {
			  return new PrimitiveNodeStream( LongStream.of( array ) );
		 }

		 public static PrimitiveNodeStream Of( object list )
		 {
			  if ( list == null )
			  {
					return _empty;
			  }
			  if ( list is System.Collections.IList )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return new PrimitiveNodeStream( ( ( IList<Node> ) list ).Select( Node::getId ) );
			  }
			  else if ( list is Node[] )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return new PrimitiveNodeStream( java.util.( Node[] ) list.Select( Node::getId ) );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "Can not convert to stream: %s", list.GetType().FullName ) );
		 }

		 public override IEnumerator<VirtualNodeValue> Iterator()
		 {
			  return Inner.mapToObj( ( System.Func<long, VirtualNodeValue> ) VirtualValues.node ).GetEnumerator();
		 }

		 private static readonly PrimitiveNodeStream _empty = new PrimitiveNodeStream( LongStream.empty() );
	}

}