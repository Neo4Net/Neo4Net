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
namespace Neo4Net.Server.rest.web
{

	public class CustomStatusType : Response.StatusType
	{
		 public static readonly Response.StatusType Unprocessable = new CustomStatusType( 422, "Unprocessable Entity" );

		 private readonly int _code;
		 private readonly string _reason;
		 private readonly Family _family;

		 public CustomStatusType( int code, string reason )
		 {
			  this._code = code;
			  this._reason = reason;
			  switch ( code / 100 )
			  {
			  case 1:
					this._family = Family.INFORMATIONAL;
					break;
			  case 2:
					this._family = Family.SUCCESSFUL;
					break;
			  case 3:
					this._family = Family.REDIRECTION;
					break;
			  case 4:
					this._family = Family.CLIENT_ERROR;
					break;
			  case 5:
					this._family = Family.SERVER_ERROR;
					break;
			  default:
					this._family = Family.OTHER;
					break;
			  }
		 }

		 public override int StatusCode
		 {
			 get
			 {
				  return _code;
			 }
		 }

		 public override Family Family
		 {
			 get
			 {
				  return _family;
			 }
		 }

		 public override string ReasonPhrase
		 {
			 get
			 {
				  return _reason;
			 }
		 }
	}

}