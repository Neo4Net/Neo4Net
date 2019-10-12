using System.Diagnostics;

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
namespace Org.Neo4j.Cypher.@internal.codegen
{
	using CursorFactory = Org.Neo4j.@internal.Kernel.Api.CursorFactory;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cypher.@internal.codegen.CompiledConversionUtils.makeValueNeoSafe;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exact;

	/// <summary>
	/// Utility for dealing with indexes from compiled code
	/// </summary>
	public sealed class CompiledIndexUtils
	{
		 /// <summary>
		 /// Do not instantiate this class
		 /// </summary>
		 private CompiledIndexUtils()
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Performs an index seek.
		 /// </summary>
		 /// <param name="read"> The Read instance to use for seeking </param>
		 /// <param name="cursors"> Used for cursor allocation </param>
		 /// <param name="index"> A reference to an index </param>
		 /// <param name="value"> The value to seek for </param>
		 /// <returns> A cursor positioned at the data found in index. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.internal.kernel.api.NodeValueIndexCursor indexSeek(org.neo4j.internal.kernel.api.Read read, org.neo4j.internal.kernel.api.CursorFactory cursors, org.neo4j.internal.kernel.api.IndexReference index, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public static NodeValueIndexCursor IndexSeek( Read read, CursorFactory cursors, IndexReference index, object value )
		 {
			  Debug.Assert( index.Properties().Length == 1 );
			  if ( value == Values.NO_VALUE || value == null )
			  {
					return Org.Neo4j.@internal.Kernel.Api.NodeValueIndexCursor_Fields.Empty;
			  }
			  else
			  {
					NodeValueIndexCursor cursor = cursors.AllocateNodeValueIndexCursor();
					IndexQuery.ExactPredicate query = exact( index.Properties()[0], makeValueNeoSafe(value) );
					read.NodeIndexSeek( index, cursor, IndexOrder.NONE, false, query );
					return cursor;
			  }
		 }
	}

}