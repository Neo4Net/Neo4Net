using System.Diagnostics;

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
	using CursorFactory = Neo4Net.Internal.Kernel.Api.CursorFactory;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cypher.Internal.codegen.CompiledConversionUtils.makeValueNeoSafe;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.IndexQuery.exact;

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
//ORIGINAL LINE: public static org.Neo4Net.internal.kernel.api.NodeValueIndexCursor indexSeek(org.Neo4Net.internal.kernel.api.Read read, org.Neo4Net.internal.kernel.api.CursorFactory cursors, org.Neo4Net.internal.kernel.api.IndexReference index, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 public static NodeValueIndexCursor IndexSeek( Read read, CursorFactory cursors, IndexReference index, object value )
		 {
			  Debug.Assert( index.Properties().Length == 1 );
			  if ( value == Values.NO_VALUE || value == null )
			  {
					return Neo4Net.Internal.Kernel.Api.NodeValueIndexCursor_Fields.Empty;
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