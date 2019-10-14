using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Before = org.junit.Before;


	using Neo4Net.Helpers.Collections;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal abstract class PruningStrategyTest
	{
		 internal Segments Segments = mock( typeof( Segments ) );
		 internal IList<SegmentFile> Files;

		 internal virtual List<SegmentFile> CreateSegmentFiles( int size )
		 {
			  List<SegmentFile> list = new List<SegmentFile>( size );
			  for ( int i = 0; i < size; i++ )
			  {
					SegmentFile file = mock( typeof( SegmentFile ) );
					when( file.Header() ).thenReturn(TestSegmentHeader(i));
					when( file.Size() ).thenReturn(1L);
					list.Add( file );
			  }
			  return list;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void mockSegmentVisitor()
		 public virtual void MockSegmentVisitor()
		 {
			  doAnswer(invocation =>
			  {
				Visitor<SegmentFile, Exception> visitor = invocation.getArgument( 0 );
				IEnumerator<SegmentFile> itr = Files.listIterator( Files.Count );
				bool terminate = false;
				while ( itr.hasPrevious() && !terminate )
				{
					 terminate = visitor.visit( itr.previous() );
				}
				return null;
			  }).when( Segments ).visitBackwards( any() );
		 }

		 private SegmentHeader TestSegmentHeader( long value )
		 {
			  return new SegmentHeader( -1, -1, value - 1, -1 );
		 }
	}

}