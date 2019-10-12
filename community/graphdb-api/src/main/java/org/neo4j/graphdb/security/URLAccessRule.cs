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
namespace Org.Neo4j.Graphdb.security
{

	using Configuration = Org.Neo4j.Graphdb.config.Configuration;

	/// <summary>
	/// A rule to evaluate if Neo4j is permitted to reach out to the specified URL (e.g. when using {@code LOAD CSV} in Cypher).
	/// </summary>
	public interface URLAccessRule
	{
		 /// <summary>
		 /// Validate this rule against the specified URL and configuration, and throw a <seealso cref="URLAccessValidationError"/>
		 /// if the URL is not permitted for access.
		 /// </summary>
		 /// <param name="configuration"> <seealso cref="Configuration"/> to validate the {@code url} against. </param>
		 /// <param name="url"> the URL being validated </param>
		 /// <returns> an updated URL that should be used for accessing the resource </returns>
		 /// <exception cref="URLAccessValidationError"> thrown if the url does not pass the validation rule </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.net.URL validate(org.neo4j.graphdb.config.Configuration configuration, java.net.URL url) throws URLAccessValidationError;
		 URL Validate( Configuration configuration, URL url );
	}

}