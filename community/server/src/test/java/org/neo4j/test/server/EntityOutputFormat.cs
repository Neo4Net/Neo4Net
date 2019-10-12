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
namespace Org.Neo4j.Test.server
{

	using ExtensionInjector = Org.Neo4j.Server.rest.repr.ExtensionInjector;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;
	using RepresentationFormat = Org.Neo4j.Server.rest.repr.RepresentationFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestAccess.serialize;

	public class EntityOutputFormat : OutputFormat
	{
		 private Representation _representation;

		 public EntityOutputFormat( RepresentationFormat format, URI baseUri, ExtensionInjector extensions ) : base( format, baseUri, extensions )
		 {
		 }

		 protected internal override Response Response( Response.ResponseBuilder response, Representation representation )
		 {
			  this._representation = representation;

			  return base.Response( response, representation );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public java.util.Map<String, Object> getResultAsMap()
		 public virtual IDictionary<string, object> ResultAsMap
		 {
			 get
			 {
				  return ( IDictionary<string, object> ) serialize( _representation );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public java.util.List<Object> getResultAsList()
		 public virtual IList<object> ResultAsList
		 {
			 get
			 {
				  return ( IList<object> ) serialize( _representation );
			 }
		 }
	}

}