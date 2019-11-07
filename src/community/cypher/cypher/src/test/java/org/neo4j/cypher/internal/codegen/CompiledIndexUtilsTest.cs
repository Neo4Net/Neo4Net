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
	using Test = org.junit.Test;

	using CursorFactory = Neo4Net.Kernel.Api.Internal.CursorFactory;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class CompiledIndexUtilsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallIndexSeek() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallIndexSeek()
		 {

			  // GIVEN
			  Read read = mock( typeof( Read ) );
			  IndexReference index = mock( typeof( IndexReference ) );
			  when( index.Properties() ).thenReturn(new int[]{ 42 });

			  // WHEN
			  CompiledIndexUtils.IndexSeek( read, mock( typeof( CursorFactory ) ), index, "hello" );

			  // THEN
			  verify( read, times( 1 ) ).nodeIndexSeek( any(), any(), any(), anyBoolean(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullInIndexSeek() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNullInIndexSeek()
		 {
			  // GIVEN
			  Read read = mock( typeof( Read ) );
			  IndexReference index = mock( typeof( IndexReference ) );
			  when( index.Properties() ).thenReturn(new int[]{ 42 });

			  // WHEN
			  NodeValueIndexCursor cursor = CompiledIndexUtils.IndexSeek( mock( typeof( Read ) ), mock( typeof( CursorFactory ) ), index, null );

			  // THEN
			  verify( read, never() ).nodeIndexSeek(any(), any(), any(), anyBoolean());
			  assertFalse( cursor.Next() );
		 }
	}

}