/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Pushtocloud
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class ProgressTrackingOutputStreamTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackSingleByteWrites() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackSingleByteWrites()
		 {
			  // given
			  Stream actual = mock( typeof( Stream ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  ProgressTrackingOutputStream.Progress progress = new ProgressTrackingOutputStream.Progress( progressListener, 0 );
			  using ( ProgressTrackingOutputStream @out = new ProgressTrackingOutputStream( actual, progress ) )
			  {
					// when
					@out.WriteByte( 10 );
			  }
			  progress.Done();

			  // then
			  verify( progressListener ).add( 1 );
			  verify( progressListener ).done();
			  verifyNoMoreInteractions( progressListener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackByteArrayWrites() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackByteArrayWrites()
		 {
			  // given
			  Stream actual = mock( typeof( Stream ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  ProgressTrackingOutputStream.Progress progress = new ProgressTrackingOutputStream.Progress( progressListener, 0 );
			  int length = 14;
			  using ( ProgressTrackingOutputStream @out = new ProgressTrackingOutputStream( actual, progress ) )
			  {
					// when
					@out.WriteByte( new sbyte[length] );
			  }
			  progress.Done();

			  // then
			  verify( progressListener ).add( length );
			  verify( progressListener ).done();
			  verifyNoMoreInteractions( progressListener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackOffsetByteArrayWrites() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackOffsetByteArrayWrites()
		 {
			  // given
			  Stream actual = mock( typeof( Stream ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  ProgressTrackingOutputStream.Progress progress = new ProgressTrackingOutputStream.Progress( progressListener, 0 );
			  int length = 5;
			  using ( ProgressTrackingOutputStream @out = new ProgressTrackingOutputStream( actual, progress ) )
			  {
					// when
					@out.Write( new sbyte[length * 2], 2, length );
			  }
			  progress.Done();

			  // then
			  verify( progressListener ).add( length );
			  verify( progressListener ).done();
			  verifyNoMoreInteractions( progressListener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrackOffsetAfterRewind() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrackOffsetAfterRewind()
		 {
			  // given
			  Stream actual = mock( typeof( Stream ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  ProgressTrackingOutputStream.Progress progress = new ProgressTrackingOutputStream.Progress( progressListener, 0 );
			  using ( ProgressTrackingOutputStream @out = new ProgressTrackingOutputStream( actual, progress ) )
			  {
					@out.WriteByte( new sbyte[20] );

					// when
					progress.RewindTo( 15 ); // i.e. the next 5 bytes we don't track
					@out.WriteByte( new sbyte[3] ); // now there should be 2 untracked bytes left
					@out.WriteByte( new sbyte[9] ); // this one should report 7
			  }
			  progress.Done();

			  // then
			  InOrder inOrder = inOrder( progressListener );
			  inOrder.verify( progressListener ).add( 20 );
			  inOrder.verify( progressListener ).add( 7 );
			  inOrder.verify( progressListener ).done();
			  verifyNoMoreInteractions( progressListener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNoteInitialPosition()
		 public virtual void ShouldNoteInitialPosition()
		 {
			  // given
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );

			  // when
			  new ProgressTrackingOutputStream.Progress( progressListener, 10 );

			  // then
			  verify( progressListener ).add( 10 );
			  verifyNoMoreInteractions( progressListener );
		 }
	}

}