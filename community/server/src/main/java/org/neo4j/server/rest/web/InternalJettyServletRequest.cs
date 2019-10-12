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
namespace Org.Neo4j.Server.rest.web
{
	using HttpFields = org.eclipse.jetty.http.HttpFields;
	using HttpURI = org.eclipse.jetty.http.HttpURI;
	using MetaData = org.eclipse.jetty.http.MetaData;
	using Request = org.eclipse.jetty.server.Request;
	using Response = org.eclipse.jetty.server.Response;


	using UTF8 = Org.Neo4j.@string.UTF8;

	public class InternalJettyServletRequest : Request
	{
		 private class Input : ServletInputStream
		 {
			 private readonly InternalJettyServletRequest _outerInstance;


			  internal readonly sbyte[] Bytes;
			  internal int Position;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ReadListener ReadListenerConflict;

			  internal Input( InternalJettyServletRequest outerInstance, string data )
			  {
				  this._outerInstance = outerInstance;
					Bytes = UTF8.encode( data );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int read() throws java.io.IOException
			  public override int Read()
			  {
					if ( Bytes.Length > Position )
					{
						 return ( int ) Bytes[Position++];
					}

					if ( ReadListenerConflict != null )
					{
						 ReadListenerConflict.onAllDataRead();
					}

					return -1;
			  }

			  public virtual int Length()
			  {
					return Bytes.Length;
			  }

			  public virtual long ContentRead()
			  {
					return ( long ) Position;
			  }

			  public override bool Finished
			  {
				  get
				  {
						return Bytes.Length == Position;
				  }
			  }

			  public override bool Ready
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override ReadListener ReadListener
			  {
				  set
				  {
						this.ReadListenerConflict = value;
						try
						{
							 value.onDataAvailable();
						}
						catch ( IOException )
						{
							 // Ignore
						}
				  }
			  }
		 }

		 private readonly IDictionary<string, object> _headers;
		 private readonly Cookie[] _cookies;
		 private readonly Input _input;
		 private readonly StreamReader _inputReader;
		 private string _contentType;
		 private readonly string _method;
		 private readonly InternalJettyServletResponse _response;

		 /// <summary>
		 /// Contains metadata for the request, for example remote address and port. </summary>
		 private readonly RequestData _requestData;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public InternalJettyServletRequest(String method, String uri, String body, InternalJettyServletResponse res, RequestData requestData) throws java.io.UnsupportedEncodingException
		 public InternalJettyServletRequest( string method, string uri, string body, InternalJettyServletResponse res, RequestData requestData ) : this( method, new HttpURI( uri ), body, new Cookie[] {}, MediaType.APPLICATION_JSON, StandardCharsets.UTF_8.name(), res, requestData )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public InternalJettyServletRequest(String method, org.eclipse.jetty.http.HttpURI uri, String body, javax.servlet.http.Cookie[] cookies, String contentType, String encoding, InternalJettyServletResponse res, RequestData requestData) throws java.io.UnsupportedEncodingException
		 public InternalJettyServletRequest( string method, HttpURI uri, string body, Cookie[] cookies, string contentType, string encoding, InternalJettyServletResponse res, RequestData requestData ) : base( null, null )
		 {

			  this._input = new Input( this, body );
			  this._inputReader = new StreamReader( new StringReader( body ) );

			  this._contentType = contentType;
			  this._cookies = cookies;
			  this._method = method;
			  this._response = res;
			  this._requestData = requestData;

			  this._headers = new Dictionary<string, object>();

			  CharacterEncoding = encoding;
			  DispatcherType = DispatcherType.REQUEST;

			  MetaData.Request request = new MetaData.Request( new HttpFields() );
			  request.Method = method;
			  request.URI = uri;
			  MetaData = request;
		 }

		 public override int ContentLength
		 {
			 get
			 {
				  return _input.length();
			 }
		 }

		 public override string ContentType
		 {
			 get
			 {
				  return _contentType;
			 }
			 set
			 {
				  this._contentType = value;
			 }
		 }


		 public override long ContentRead
		 {
			 get
			 {
				  return _input.contentRead();
			 }
		 }

		 public override ServletInputStream InputStream
		 {
			 get
			 {
				  return _input;
			 }
		 }

		 public override string Protocol
		 {
			 get
			 {
				  return "HTTP/1.1";
			 }
		 }

		 public override StreamReader Reader
		 {
			 get
			 {
				  return _inputReader;
			 }
		 }

		 public override string RemoteAddr
		 {
			 get
			 {
				  return _requestData.remoteAddress;
			 }
		 }

		 public override string RemoteHost
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Remote host-name lookup might prove expensive, " + "this should be explicitly considered." );
			 }
		 }

		 public override bool Secure
		 {
			 get
			 {
				  return _requestData.isSecure;
			 }
		 }

		 public override int RemotePort
		 {
			 get
			 {
				  return _requestData.remotePort;
			 }
		 }

		 public override string LocalName
		 {
			 get
			 {
				  return _requestData.localName;
			 }
		 }

		 public override string LocalAddr
		 {
			 get
			 {
				  return _requestData.localAddress;
			 }
		 }

		 public override int LocalPort
		 {
			 get
			 {
				  return _requestData.localPort;
			 }
		 }

		 public override string AuthType
		 {
			 get
			 {
				  return _requestData.authType;
			 }
		 }

		 public override Cookie[] Cookies
		 {
			 get
			 {
				  return _cookies;
			 }
		 }

		 public virtual void AddHeader( string header, string value )
		 {
			  _headers[header] = value;
		 }

		 public override bool AsyncSupported
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override bool AsyncStarted
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override long GetDateHeader( string name )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					return ( long? ) _headers[name].Value;
			  }
			  return -1;
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

		 public override IEnumerator<string> GetHeaders( string name )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					object value = _headers[name];
					if ( value is System.Collections.ICollection )
					{
						 return Collections.enumeration( ( ICollection<string> ) value );
					}
					else
					{
						 return Collections.enumeration( Collections.singleton( ( string ) value ) );
					}
			  }
			  return null;
		 }

		 public override IEnumerator<string> HeaderNames
		 {
			 get
			 {
				  return Collections.enumeration( _headers.Keys );
			 }
		 }

		 public override int GetIntHeader( string name )
		 {
			  if ( _headers.ContainsKey( name ) )
			  {
					return ( int? ) _headers[name].Value;
			  }
			  return -1;
		 }

		 public override string Method
		 {
			 get
			 {
				  return _method;
			 }
		 }

		 public override Response Response
		 {
			 get
			 {
				  return _response;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0} {1} {2}\n{3}", System.Reflection.MethodInfo, this.HttpURI, Protocol, HttpFields );
		 }

		 public class RequestData
		 {
			  public readonly string RemoteAddress;
			  public readonly bool IsSecure;
			  public readonly int RemotePort;
			  public readonly string LocalName;
			  public readonly string LocalAddress;
			  public readonly int LocalPort;
			  public readonly string AuthType;

			  public RequestData( string remoteAddress, bool isSecure, int remotePort, string localName, string localAddress, int localPort, string authType )
			  {
					this.RemoteAddress = remoteAddress;
					this.IsSecure = isSecure;
					this.RemotePort = remotePort;
					this.LocalName = localName;
					this.LocalAddress = localAddress;
					this.LocalPort = localPort;
					this.AuthType = authType;
			  }

			  public static RequestData From( HttpServletRequest req )
			  {
					return new RequestData( req.RemoteAddr, req.Secure, req.RemotePort, req.LocalName, req.LocalAddr, req.LocalPort, req.AuthType == null ? "" : req.AuthType );
			  }
		 }
	}

}