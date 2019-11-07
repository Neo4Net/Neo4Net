using System;

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
namespace Neo4Net.Values
{
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Neo4Net.Values.Storable;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

	/// <summary>
	/// Writer of any values.
	/// </summary>
	public interface AnyValueWriter<E> : ValueWriter<E> where E : Exception
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeNodeReference(long nodeId) throws E;
		 void WriteNodeReference( long nodeId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeNode(long nodeId, Neo4Net.values.storable.TextArray labels, Neo4Net.values.virtual.MapValue properties) throws E;
		 void WriteNode( long nodeId, TextArray labels, MapValue properties );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeRelationshipReference(long relId) throws E;
		 void WriteRelationshipReference( long relId );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writeRelationship(long relId, long startNodeId, long endNodeId, Neo4Net.values.storable.TextValue type, Neo4Net.values.virtual.MapValue properties) throws E;
		 void WriteRelationship( long relId, long startNodeId, long endNodeId, TextValue type, MapValue properties );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginMap(int size) throws E;
		 void BeginMap( int size );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void endMap() throws E;
		 void EndMap();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void beginList(int size) throws E;
		 void BeginList( int size );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void endList() throws E;
		 void EndList();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void writePath(Neo4Net.values.virtual.NodeValue[] nodes, Neo4Net.values.virtual.RelationshipValue[] relationships) throws E;
		 void WritePath( NodeValue[] nodes, RelationshipValue[] relationships );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void writeVirtualNodeHack(Object node)
	//	 {
	//		  // do nothing, this is an ugly hack.
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void writeVirtualRelationshipHack(Object relationship)
	//	 {
	//		  // do nothing, this is an ugly hack.
	//	 }
	}

}