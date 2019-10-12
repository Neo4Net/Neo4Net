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
namespace Neo4Net.Server.rest
{

	internal class RequestData
	{
		 private string _payload;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public string UriConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public string MethodConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public int StatusConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public string EntityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public IDictionary<string, string> RequestHeadersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public IDictionary<string, string> ResponseHeadersConflict;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setPayload(final String payload)
		 public virtual string Payload
		 {
			 set
			 {
				  this._payload = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setUri(final String uri)
		 public virtual string Uri
		 {
			 set
			 {
				  this.UriConflict = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setMethod(final String method)
		 public virtual string Method
		 {
			 set
			 {
				  this.MethodConflict = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setStatus(final int responseCode)
		 public virtual int Status
		 {
			 set
			 {
				  this.StatusConflict = value;
   
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setEntity(final String entity)
		 public virtual string Entity
		 {
			 set
			 {
				  this.EntityConflict = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setResponseHeaders(final java.util.Map<String, String> response)
		 public virtual IDictionary<string, string> ResponseHeaders
		 {
			 set
			 {
				  ResponseHeadersConflict = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void setRequestHeaders(final java.util.Map<String, String> request)
		 public virtual IDictionary<string, string> RequestHeaders
		 {
			 set
			 {
				  RequestHeadersConflict = value;
			 }
		 }

		 public override string ToString()
		 {
			  return "DocumentationData [payload=" + _payload + ", uri=" + UriConflict + ", method=" + MethodConflict + ", status=" + StatusConflict + ", entity=" + EntityConflict + ", requestHeaders=" + RequestHeadersConflict + ", responseHeaders=" + ResponseHeadersConflict + "]";
		 }
	}

}