﻿using System.Collections.Generic;

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
namespace Neo4Net.Server.rest
{

	using Test = org.junit.Test;

	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using GraphDescription = Neo4Net.Test.GraphDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.domain.JsonHelper.readJson;

	public class ListPropertyKeysIT : AbstractRestFunctionalTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("List all property keys.") @GraphDescription.Graph(nodes = { @GraphDescription.NODE(name = "a", setNameProperty = true), @GraphDescription.NODE(name = "b", setNameProperty = true), @GraphDescription.NODE(name = "c", setNameProperty = true) }) public void list_all_property_keys_ever_used() throws Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("List all property keys.")]
		 public virtual void ListAllPropertyKeysEverUsed()
		 {
			  Data.get();
			  string uri = PropertyKeysUri;
			  string body = GenConflict.get().expectedStatus(200).get(uri).entity();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> parsed = Neo4Net.helpers.collection.Iterables.asSet((java.util.List<?>) readJson(body));
			  ISet<object> parsed = Iterables.asSet( ( IList<object> ) readJson( body ) );
			  assertTrue( parsed.Contains( "name" ) );
		 }
	}

}