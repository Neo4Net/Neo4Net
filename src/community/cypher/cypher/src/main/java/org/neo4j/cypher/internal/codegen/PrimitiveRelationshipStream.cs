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
namespace Neo4Net.Cypher.Internal.codegen
{

	using Relationship = Neo4Net.GraphDb.Relationship;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public class PrimitiveRelationshipStream : PrimitiveEntityStream<VirtualRelationshipValue>
	{
		 public PrimitiveRelationshipStream( LongStream inner ) : base( inner )
		 {
		 }

		 public static PrimitiveRelationshipStream Of( long[] array )
		 {
			  return new PrimitiveRelationshipStream( LongStream.of( array ) );
		 }

		 public static PrimitiveRelationshipStream Of( object list )
		 {
			  if ( null == list )
			  {
					return _empty;
			  }
			  else if ( list is System.Collections.IList )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return new PrimitiveRelationshipStream( ( ( IList<Relationship> ) list ).Select( Relationship::getId ) );
			  }
			  else if ( list is Relationship[] )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					return new PrimitiveRelationshipStream( java.util.( Relationship[] ) list.Select( Relationship::getId ) );
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new System.ArgumentException( format( "Can not convert to stream: %s", list.GetType().FullName ) );
		 }

		 public override IEnumerator<VirtualRelationshipValue> Iterator()
		 {
			  return Inner.mapToObj( ( System.Func<long, VirtualRelationshipValue> ) VirtualValues.relationship ).GetEnumerator();
		 }

		 private static readonly PrimitiveRelationshipStream _empty = new PrimitiveRelationshipStream( LongStream.empty() );
	}

}