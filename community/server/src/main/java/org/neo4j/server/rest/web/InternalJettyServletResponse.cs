using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
namespace Org.Neo4j.Server.rest.web
{
	using HttpFields = org.eclipse.jetty.http.HttpFields;
	using Response = org.eclipse.jetty.server.Response;


	public class InternalJettyServletResponse : Response
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_output = new Output( this );
		}


		 private class Output : ServletOutputStream
		 {
			 private readonly InternalJettyServletResponse _outerInstance;

			 public Output( InternalJettyServletResponse outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  internal readonly MemoryStream Baos = new MemoryStream();

			  public override void Write( int c )
			  {
					Baos.WriteByte( c );
			  }

			  public override string ToString()
			  {
					try
					{
						 Baos.Flush();
						 return Baos.ToString( StandardCharsets.UTF_8.name() );
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
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

		 private readonly IDictionary<string, object> _headers = new Dictionary<string, object>();
		 private Output _output;
		 private int _status = -1;
		 private string _message = "";

		 public InternalJettyServletResponse() : base(null, null)
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
		 }

		 public override void AddCookie( Cookie cookie )
		 {
			  // TODO Auto-generated method stub
		 }

		 public override string EncodeURL( string url )
		 {
			  // TODO Auto-generated method stub
			  return null;
		 }

		 public override void SendError( int sc )
		 {
			  SendError( sc, null );
		 }

		 public override void SendError( int code, string message )
		 {
			  SetStatus( code, message );
		 }

		 public override void SendRedirect( string location )
		 {
			  Status = 304;
			  AddHeader( "location", location );
		 }

		 public override bool ContainsHeader( string name )
		 {
			  return _headers.ContainsKey( name );
		 }

		 public override void SetDateHeader( string name, long date )
		 {
			  _headers[name] = date;
		 }

		 public override void AddDateHeader( string name, long date )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					_headers[name] = date;
			  }
		 }

		 public override void AddHeader( string name, string value )
		 {
			  SetHeader( name, value );
		 }

		 public override void SetHeader( string name, string value )
		 {
			  _headers[name] = value;
		 }

		 public override void SetIntHeader( string name, int value )
		 {
			  _headers[name] = value;
		 }

		 public override void AddIntHeader( string name, int value )
		 {
			  SetIntHeader( name, value );
		 }

		 public override string GetHeader( string name )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					object value = _headers[name];
					if ( value is string )
					{
						 return ( string ) value;
					}
					else if ( value is System.Collections.ICollection )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return ((java.util.Collection<?>) value).iterator().next().toString();
						 return ( ( ICollection<object> ) value ).GetEnumerator().next().ToString();
					}
					else
					{
						 return value.ToString();
					}
			  }

			  return null;
		 }

		 public virtual IDictionary<string, object> Headers
		 {
			 get
			 {
				  return _headers;
			 }
		 }

		 public override ICollection<string> getHeaders( string name )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					object value = _headers[name];
					if ( value is System.Collections.ICollection )
					{
						 return ( ICollection<string> ) value;
					}
					else
					{
						 return Collections.singleton( ( string ) value );
					}
			  }
			  return null;
		 }

		 public override int Status
		 {
			 set
			 {
				  _status = value;
			 }
			 get
			 {
				  return _status;
			 }
		 }

		 public override void setStatus( int sc, string sm )
		 {
			  _status = sc;
			  _message = sm;
		 }


		 public override string Reason
		 {
			 get
			 {
				  return _message;
			 }
		 }

		 public override ServletOutputStream OutputStream
		 {
			 get
			 {
				  return _output;
			 }
		 }

		 public override bool Writing
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override PrintWriter Writer
		 {
			 get
			 {
				  return new PrintWriter( new StreamWriter( _output, Encoding.UTF8 ) );
			 }
		 }

		 public override string CharacterEncoding
		 {
			 set
			 {
   
			 }
		 }

		 public override int ContentLength
		 {
			 set
			 {
			 }
		 }

		 public override long LongContentLength
		 {
			 set
			 {
			 }
		 }

		 public override string ContentType
		 {
			 set
			 {
			 }
		 }

		 public override int BufferSize
		 {
			 set
			 {
			 }
			 get
			 {
				  return -1;
			 }
		 }


		 public override void FlushBuffer()
		 {
		 }

		 public override string ToString()
		 {
			  return null;
		 }

		 public override HttpFields HttpFields
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public override long ContentCount
		 {
			 get
			 {
				  return 1L;
			 }
		 }

		 public virtual void Complete()
		 {
		 }

		 public override Locale Locale
		 {
			 set
			 {
			 }
		 }

		 public override bool Committed
		 {
			 get
			 {
				  return false;
			 }
		 }

	}

}