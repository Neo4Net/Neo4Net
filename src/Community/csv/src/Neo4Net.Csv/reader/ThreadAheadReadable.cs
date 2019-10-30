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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// Like an ordinary <seealso cref="CharReadable"/>, it's just that the reading happens in a separate thread, so when
	/// a consumer wants to <seealso cref="read(SectionedCharBuffer, int)"/> more data it's already available, merely a memcopy away.
	/// </summary>
	public class ThreadAheadReadable : ThreadAhead, CharReadable
	{
		 private readonly CharReadable _actual;
		 private SectionedCharBuffer _theOtherBuffer;

		 private string _sourceDescription;
		 // the variable below is read and changed in both the ahead thread and the caller,
		 // but doesn't have to be volatile since it piggy-backs off of hasReadAhead.
		 private string _newSourceDescription;

		 private ThreadAheadReadable( CharReadable actual, int bufferSize ) : base( actual )
		 {
			  this._actual = actual;
			  this._theOtherBuffer = new SectionedCharBuffer( bufferSize );
			  this._sourceDescription = actual.SourceDescription();
			  start();
		 }

		 /// <summary>
		 /// The one calling read doesn't actually read, since reading is up to the thread in here.
		 /// Instead the caller just waits for this thread to have fully read the next buffer and
		 /// flips over to that buffer, returning it.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException
		 public override SectionedCharBuffer Read( SectionedCharBuffer buffer, int from )
		 {
			  WaitUntilReadAhead();

			  // flip the buffers
			  SectionedCharBuffer resultBuffer = _theOtherBuffer;
			  buffer.Compact( resultBuffer, from );
			  _theOtherBuffer = buffer;

			  // make any change in source official
			  if ( !string.ReferenceEquals( _newSourceDescription, null ) )
			  {
					_sourceDescription = _newSourceDescription;
					_newSourceDescription = null;
			  }

			  PokeReader();
			  return resultBuffer;
		 }

		 public override int Read( char[] into, int offset, int length )
		 {
			  throw new System.NotSupportedException( "Unsupported for now" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean readAhead() throws java.io.IOException
		 protected internal override bool ReadAhead()
		 {
			  _theOtherBuffer = _actual.read( _theOtherBuffer, _theOtherBuffer.front() );
			  string sourceDescriptionAfterRead = _actual.sourceDescription();
			  if ( !_sourceDescription.Equals( sourceDescriptionAfterRead ) )
			  {
					_newSourceDescription = sourceDescriptionAfterRead;
			  }

			  return _theOtherBuffer.hasAvailable();
		 }

		 public override long Position()
		 {
			  return _actual.position();
		 }

		 public override string SourceDescription()
		 { // Returns the source information of where this reader is perceived to be. The fact that this
			  // thing reads ahead should be visible in this description.
			  return _sourceDescription;
		 }

		 public static CharReadable ThreadAhead( CharReadable actual, int bufferSize )
		 {
			  return new ThreadAheadReadable( actual, bufferSize );
		 }

		 public override long Length()
		 {
			  return _actual.length();
		 }
	}

}