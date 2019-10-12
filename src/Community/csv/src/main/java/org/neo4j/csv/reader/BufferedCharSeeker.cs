using System;
using System.Globalization;

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
namespace Neo4Net.Csv.Reader
{

	using CSVHeaderInformation = Neo4Net.Values.Storable.CSVHeaderInformation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.Mark.END_OF_LINE_CHARACTER;

	/// <summary>
	/// Much like a <seealso cref="System.IO.StreamReader_BufferedReader"/> for a <seealso cref="Reader"/>.
	/// </summary>
	public class BufferedCharSeeker : CharSeeker
	{
		 private const char EOL_CHAR = '\n';
		 private const char EOL_CHAR_2 = '\r';
		 private static readonly char _eofChar = ( char ) - 1;
		 private const char BACK_SLASH = '\\';

		 private char[] _buffer;
		 private int _dataLength;
		 private int _dataCapacity;

		 // index into the buffer character array to read the next time nextChar() is called
		 private int _bufferPos;
		 private int _bufferStartPos;
		 // last index (effectively length) of characters in use in the buffer
		 private int _bufferEnd;
		 // bufferPos denoting the start of this current line that we're reading
		 private int _lineStartPos;
		 // bufferPos when we started reading the current field
		 private int _seekStartPos;
		 // 1-based value of which logical line we're reading a.t.m.
		 private int _lineNumber;
		 // flag to know if we've read to the end
		 private bool _eof;
		 // char to recognize as quote start/end
		 private readonly char _quoteChar;
		 // this absolute position + bufferPos is the current position in the source we're reading
		 private long _absoluteBufferStartPosition;
		 private string _sourceDescription;
		 private readonly bool _multilineFields;
		 private readonly bool _legacyStyleQuoting;
		 private readonly Source _source;
		 private Source_Chunk _currentChunk;
		 private readonly bool _trim;

		 public BufferedCharSeeker( Source source, Configuration config )
		 {
			  this._source = source;
			  this._quoteChar = config.QuotationCharacter();
			  this._lineStartPos = this._bufferPos;
			  this._multilineFields = config.MultilineFields();
			  this._legacyStyleQuoting = config.LegacyStyleQuoting();
			  this._trim = GetTrimStringIgnoreErrors( config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean seek(Mark mark, int untilChar) throws java.io.IOException
		 public override bool Seek( Mark mark, int untilChar )
		 {
			  if ( _eof )
			  { // We're at the end
					return Eof( mark );
			  }

			  // Keep a start position in case we need to further fill the buffer in nextChar, a value can at maximum be the
			  // whole buffer, so max one fill per value is supported.
			  _seekStartPos = _bufferPos; // seekStartPos updated in nextChar if buffer flips over, that's why it's a member
			  int ch;
			  int endOffset = 1;
			  int skippedChars = 0;
			  int quoteDepth = 0;
			  int quoteStartLine = 0;
			  bool isQuoted = false;

			  while ( !_eof )
			  {
					ch = NextChar( skippedChars );
					if ( quoteDepth == 0 )
					{ // In normal mode, i.e. not within quotes
						 if ( ch == untilChar )
						 { // We found a delimiter, set marker and return true
							  return SetMark( mark, endOffset, skippedChars, ch, isQuoted );
						 }
						 else if ( _trim && IsWhitespace( ch ) )
						 { // Only check for left+trim whitespace as long as we haven't found a non-whitespace character
							  if ( _seekStartPos == _bufferPos - 1 )
							  { // We found a whitespace, which is before the first non-whitespace of the value and we've been told to trim that off
									_seekStartPos++;
							  }
						 }
						 else if ( ( char )ch == _quoteChar && _seekStartPos == _bufferPos - 1 )
						 { // We found a quote, which was the first of the value, skip it and switch mode
							  quoteDepth++;
							  isQuoted = true;
							  _seekStartPos++;
							  quoteStartLine = _lineNumber;
						 }
						 else if ( IsNewLine( ch ) )
						 { // Encountered newline, done for now
							  if ( _bufferPos - 1 == _lineStartPos )
							  { // We're at the start of this read so just skip it
									_seekStartPos++;
									_lineStartPos++;
									continue;
							  }
							  break;
						 }
						 else if ( isQuoted )
						 { // This value is quoted, i.e. started with a quote and has also seen a quote
							  throw new DataAfterQuoteException( this, new string( _buffer, _seekStartPos, _bufferPos - _seekStartPos ) );
						 }
						 // else this is a character to include as part of the current value
					}
					else
					{ // In quoted mode, i.e. within quotes
						 if ( ( char )ch == _quoteChar )
						 { // Found a quote within a quote, peek at next char
							  int nextCh = PeekChar( skippedChars );
							  if ( ( char )nextCh == _quoteChar )
							  { // Found a double quote, skip it and we're going down one more quote depth (quote-in-quote)
									RepositionChar( _bufferPos++, ++skippedChars );
							  }
							  else
							  { // Found an ending quote, skip it and switch mode
									endOffset++;
									quoteDepth--;
							  }
						 }
						 else if ( IsNewLine( ch ) )
						 { // Found a new line inside a quotation...
							  if ( !_multilineFields )
							  { // ...but we are configured to disallow it
									throw new IllegalMultilineFieldException( this );
							  }
							  // ... it's OK, just keep going
							  if ( ( char )ch == EOL_CHAR )
							  {
									_lineNumber++;
							  }
						 }
						 else if ( ( char )ch == BACK_SLASH && _legacyStyleQuoting )
						 { // Legacy concern, support java style quote encoding
							  int nextCh = PeekChar( skippedChars );
							  if ( ( char )nextCh == _quoteChar || ( char )nextCh == BACK_SLASH )
							  { // Found a slash encoded quote
									RepositionChar( _bufferPos++, ++skippedChars );
							  }
						 }
						 else if ( _eof )
						 {
							  // We have an open quote but have reached the end of the file, this is a formatting error
							  throw new MissingEndQuoteException( this, quoteStartLine, _quoteChar );
						 }
					}
			  }

			  int valueLength = _bufferPos - _seekStartPos - 1;
			  if ( _eof && valueLength == 0 && _seekStartPos == _lineStartPos )
			  { // We didn't find any of the characters sought for
					return Eof( mark );
			  }

			  // We found the last value of the line or stream
			  _lineNumber++;
			  _lineStartPos = _bufferPos;
			  return SetMark( mark, endOffset, skippedChars, END_OF_LINE_CHARACTER, isQuoted );
		 }

		 public override EXTRACTOR Extract<EXTRACTOR>( Mark mark, EXTRACTOR extractor )
		 {
			  return Extract( mark, extractor, null );
		 }

		 private bool SetMark( Mark mark, int endOffset, int skippedChars, int ch, bool isQuoted )
		 {
			  int pos = ( _trim ? Rtrim() : _bufferPos ) - endOffset - skippedChars;
			  mark.Set( _seekStartPos, pos, ch, isQuoted );
			  return true;
		 }

		 /// <summary>
		 /// Starting from the current position, <seealso cref="bufferPos"/>, scan backwards as long as whitespace is found.
		 /// Although it cannot scan further back than the start of this field is, i.e. <seealso cref="seekStartPos"/>.
		 /// </summary>
		 /// <returns> the right index of the value to pass into <seealso cref="Mark"/>. This is only called if <seealso cref="Configuration.trimStrings()"/> is {@code true}. </returns>
		 private int Rtrim()
		 {
			  int index = _bufferPos;
			  while ( index - 1 > _seekStartPos && IsWhitespace( _buffer[index - 1 - 1] ) )
			  {
					index--;
			  }
			  return index;
		 }

		 private bool IsWhitespace( int ch )
		 {
			  return ch == ' ' || ch == UnicodeCategory.SpaceSeparator || ch == UnicodeCategory.ParagraphSeparator || ch == '\u00A0' || ch == '\u001C' || ch == '\u001D' || ch == '\u001E' || ch == '\u001F' || ch == '\u2007' || ch == '\u202F' || ch == '\t';

		 }

		 private void RepositionChar( int offset, int stepsBack )
		 {
			  // We reposition characters because we might have skipped some along the way, double-quotes and what not.
			  // We want to take an as little hit as possible for that, so we reposition each character as long as
			  // we're still reading the same value. All other values will not have to take any hit of skipped chars
			  // for this particular value.
			  _buffer[offset - stepsBack] = _buffer[offset];
		 }

		 private bool IsNewLine( int ch )
		 {
			  return ( char )ch == EOL_CHAR || ( char )ch == EOL_CHAR_2;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int peekChar(int skippedChars) throws java.io.IOException
		 private int PeekChar( int skippedChars )
		 {
			  int ch = NextChar( skippedChars );
			  try
			  {
					return ch;
			  }
			  finally
			  {
					if ( ( char )ch != _eofChar )
					{
						 _bufferPos--;
					}
			  }
		 }

		 private bool Eof( Mark mark )
		 {
			  mark.Set( -1, -1, Mark.END_OF_LINE_CHARACTER, false );
			  return false;
		 }

		 private static bool GetTrimStringIgnoreErrors( Configuration config )
		 {
			  try
			  {
					return config.TrimStrings();
			  }
			  catch ( Exception )
			  {
					// Cypher compatibility can result in older Cypher 2.3 code being passed here with older implementations of
					// Configuration. So we need to ignore the fact that those implementations do not include trimStrings().
					return Configuration_Fields.Default.trimStrings();
			  }
		 }

		 public override EXTRACTOR Extract<EXTRACTOR>( Mark mark, EXTRACTOR extractor, CSVHeaderInformation optionalData )
		 {
			  if ( !TryExtract( mark, extractor, optionalData ) )
			  {
					throw new System.InvalidOperationException( extractor + " didn't extract value for " + mark + ". For values which are optional please use tryExtract method instead" );
			  }
			  return extractor;
		 }

		 public override bool TryExtract<T1>( Mark mark, Extractor<T1> extractor, CSVHeaderInformation optionalData )
		 {
			  int from = mark.StartPosition();
			  int to = mark.Position();
			  return extractor.Extract( _buffer, from, to - from, mark.Quoted, optionalData );
		 }

		 public override bool TryExtract<T1>( Mark mark, Extractor<T1> extractor )
		 {
			  return TryExtract( mark, extractor, null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int nextChar(int skippedChars) throws java.io.IOException
		 private int NextChar( int skippedChars )
		 {
			  int ch;
			  if ( _bufferPos < _bufferEnd || FillBuffer() )
			  {
					ch = _buffer[_bufferPos];
			  }
			  else
			  {
					ch = _eofChar;
					_eof = true;
			  }

			  if ( skippedChars > 0 )
			  {
					RepositionChar( _bufferPos, skippedChars );
			  }
			  _bufferPos++;
			  return ch;
		 }

		 /// <returns> {@code true} if something was read, otherwise {@code false} which means that we reached EOF. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean fillBuffer() throws java.io.IOException
		 private bool FillBuffer()
		 {
			  bool first = _currentChunk == null;

			  if ( !first )
			  {
					if ( _bufferPos - _seekStartPos >= _dataCapacity )
					{
						 throw new BufferOverflowException( "Tried to read a field larger than buffer size " + _dataLength + ". A common cause of this is that a field has an unterminated " + "quote and so will try to seek until the next quote, which ever line it may be on." + " This should not happen if multi-line fields are disabled, given that the fields contains " + "no new-line characters. This field started at " + SourceDescription() + ":" + LineNumber() );
					}
			  }

			  _absoluteBufferStartPosition += _dataLength;

			  // Fill the buffer with new characters
			  Source_Chunk nextChunk = _source.nextChunk( first ? -1 : _seekStartPos );
			  if ( nextChunk == Source.EMPTY_CHUNK )
			  {
					return false;
			  }

			  _buffer = nextChunk.Data();
			  _dataLength = nextChunk.Length();
			  _dataCapacity = nextChunk.MaxFieldSize();
			  _bufferPos = nextChunk.StartPosition();
			  _bufferStartPos = _bufferPos;
			  _bufferEnd = _bufferPos + _dataLength;
			  int shift = _seekStartPos - nextChunk.BackPosition();
			  _seekStartPos = nextChunk.BackPosition();
			  if ( first )
			  {
					_lineStartPos = _seekStartPos;
			  }
			  else
			  {
					_lineStartPos -= shift;
			  }
			  string sourceDescriptionAfterRead = nextChunk.SourceDescription();
			  if ( !sourceDescriptionAfterRead.Equals( _sourceDescription ) )
			  { // We moved over to a new source, reset line number
					_lineNumber = 0;
					_sourceDescription = sourceDescriptionAfterRead;
			  }
			  _currentChunk = nextChunk;
			  return _dataLength > 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _source.Dispose();
		 }

		 public override long Position()
		 {
			  return _absoluteBufferStartPosition + ( _bufferPos - _bufferStartPos );
		 }

		 public override string SourceDescription()
		 {
			  return _sourceDescription;
		 }

		 public virtual long LineNumber()
		 {
			  return _lineNumber;
		 }

		 public override string ToString()
		 {
			  return format( "%s[source:%s, position:%d, line:%d]", this.GetType().Name, SourceDescription(), Position(), LineNumber() );
		 }

		 public static bool IsEolChar( char c )
		 {
			  return c == EOL_CHAR || c == EOL_CHAR_2;
		 }
	}

}