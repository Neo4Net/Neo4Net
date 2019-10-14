using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.batch
{

	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;

	/*
	 * Because the batch operation API operates on the HTTP abstraction
	 * level, we do not use our normal serialization system for serializing
	 * its' results.
	 *
	 * Doing so would require us to de-serialize each JSON response we get from
	 * each operation, and we would have to extend our current type safe serialization
	 * system to incorporate arbitrary responses.
	 */
	public class BatchOperationResults
	{
		 private const string CLOSING_BRACKET = "]";
		 private const string OPENING_BRACKET = "[";
		 private const string OPENING_CURLY = "{";
		 private const string CLOSING_CURLY = "}";
		 private const string COMMA = ",";

		 private StringWriter _results = new StringWriter();
		 private bool _firstResult = true;
		 private IDictionary<int, string> _locations = new Dictionary<int, string>();

		 public BatchOperationResults()
		 {
			  _results.append( OPENING_BRACKET );
		 }

		 public virtual void AddOperationResult( string from, int? id, string body, string location )
		 {
			  if ( _firstResult )
			  {
					_firstResult = false;
			  }
			  else
			  {
					_results.append( ',' );
			  }

			  _results.append( OPENING_CURLY );

			  if ( id != null )
			  {
					_results.append( "\"id\":" ).append( id.ToString() ).append(COMMA);
			  }

			  if ( !string.ReferenceEquals( location, null ) )
			  {
					_locations[id] = location;
					_results.append( "\"location\":" ).append( JsonHelper.createJsonFrom( location ) ).append( COMMA );
			  }

			  if ( !string.ReferenceEquals( body, null ) && body.Length != 0 )
			  {
					_results.append( "\"body\":" ).append( body ).append( COMMA );
			  }

			  _results.append( "\"from\":" ).append( JsonHelper.createJsonFrom( from ) );

			  _results.append( CLOSING_CURLY );
		 }

		 public virtual IDictionary<int, string> Locations
		 {
			 get
			 {
				  return _locations;
			 }
		 }

		 public virtual string ToJSON()
		 {
			  _results.append( CLOSING_BRACKET );
			  return _results.ToString();
		 }
	}

}