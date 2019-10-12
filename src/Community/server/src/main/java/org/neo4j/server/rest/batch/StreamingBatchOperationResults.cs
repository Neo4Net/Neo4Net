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
namespace Neo4Net.Server.rest.batch
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;


	/*
	 * Because the batch operation API operates on the HTTP abstraction
	 * level, we do not use our normal serialization system for serializing
	 * its' results.
	 *
	 * Doing so would require us to de-serialize each JSON response we get from
	 * each operation, and we would have to extend our current type safe serialization
	 * system to incorporate arbitrary responses.
	 */
	public class StreamingBatchOperationResults
	{
		 public const int HEAD_BUFFER = 10;
		 public const int IS_ERROR = -1;
		 private readonly IDictionary<int, string> _locations = new Dictionary<int, string>();
		 private readonly JsonGenerator _g;
		 private readonly ServletOutputStream _output;
		 private MemoryStream _errorStream;
		 private int _bytesWritten;
		 private char[] _head = new char[HEAD_BUFFER];

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public StreamingBatchOperationResults(org.codehaus.jackson.JsonGenerator g, javax.servlet.ServletOutputStream output) throws java.io.IOException
		 public StreamingBatchOperationResults( JsonGenerator g, ServletOutputStream output )
		 {
			  this._g = g;
			  this._output = output;
			  g.writeStartArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startOperation(String from, System.Nullable<int> id) throws java.io.IOException
		 public virtual void StartOperation( string from, int? id )
		 {
			  _bytesWritten = 0;
			  _g.writeStartObject();
			  if ( id != null )
			  {
					_g.writeNumberField( "id", id );
			  }
			  _g.writeStringField( "from", from );
			  _g.writeRaw( ",\"body\":" );
			  _g.flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addOperationResult(int status, System.Nullable<int> id, String location) throws java.io.IOException
		 public virtual void AddOperationResult( int status, int? id, string location )
		 {
			  FinishBody();
			  if ( !string.ReferenceEquals( location, null ) )
			  {
					_locations[id] = location;
					_g.writeStringField( "location", location );
			  }
			  _g.writeNumberField( "status", status );
			  _g.writeEndObject();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void finishBody() throws java.io.IOException
		 private void FinishBody()
		 {
			  if ( _bytesWritten == 0 )
			  {
					_g.writeRaw( "null" );
			  }
			  else if ( _bytesWritten < HEAD_BUFFER )
			  {
					_g.writeRaw( _head, 0, _bytesWritten );
			  }
		 }

		 public virtual ServletOutputStream ServletOutputStream
		 {
			 get
			 {
				  return new ServletOutputStreamAnonymousInnerClass( this );
			 }
		 }

		 private class ServletOutputStreamAnonymousInnerClass : ServletOutputStream
		 {
			 private readonly StreamingBatchOperationResults _outerInstance;

			 public ServletOutputStreamAnonymousInnerClass( StreamingBatchOperationResults outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int i) throws java.io.IOException
			 public override void write( int i )
			 {
				  if ( outerInstance.redirectError( i ) )
				  {
						return;
				  }
				  outerInstance.writeChar( i );
				  _outerInstance.bytesWritten++;
				  outerInstance.checkHead();
			 }

			 public override bool Ready
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override WriteListener WriteListener
			 {
				 set
				 {
					  try
					  {
							value.onWritePossible();
					  }
					  catch ( IOException )
					  {
							// Ignore
					  }
				 }
			 }
		 }

		 private bool RedirectError( int i )
		 {
			  if ( _bytesWritten != IS_ERROR )
			  {
					return false;
			  }
			  _errorStream.WriteByte( i );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeChar(int i) throws java.io.IOException
		 private void WriteChar( int i )
		 {
			  if ( _bytesWritten < HEAD_BUFFER )
			  {
					_head[_bytesWritten] = ( char ) i;
			  }
			  else
			  {
					_output.write( i );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkHead() throws java.io.IOException
		 private void CheckHead()
		 {
			  if ( _bytesWritten == HEAD_BUFFER )
			  {
					if ( IsJson( _head ) )
					{
						 foreach ( char c in _head )
						 {
							  _output.write( c );
						 }
					}
					else
					{
						 _errorStream = new MemoryStream( 1024 );
						 foreach ( char c in _head )
						 {
							  _errorStream.WriteByte( c );
						 }
						 _bytesWritten = IS_ERROR;
					}
			  }
		 }

		 private bool IsJson( char[] head )
		 {
			  return new string( head ).matches( "\\s*([\\[\"{]|true|false).*" );
		 }

		 public virtual IDictionary<int, string> Locations
		 {
			 get
			 {
				  return _locations;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public virtual void Close()
		 {
			  _g.writeEndArray();
			  _g.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeError(int status, String message) throws java.io.IOException
		 public virtual void WriteError( int status, string message )
		 {
			  if ( _bytesWritten == 0 || _bytesWritten == IS_ERROR )
			  {
					_g.writeRaw( "null" );
			  }
			  _g.writeNumberField( "status", status );
			  if ( !string.ReferenceEquals( message, null ) && !message.Trim().Equals(Response.Status.fromStatusCode(status).ReasonPhrase) )
			  {
					_g.writeStringField( "message", message );
			  }
			  else
			  {
					if ( _errorStream != null )
					{
						 _g.writeStringField( "message", _errorStream.ToString( StandardCharsets.UTF_8.name() ) );
					}
			  }
			  _g.close();
		 }
	}

}