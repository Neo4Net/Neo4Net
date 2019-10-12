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
namespace Neo4Net.Server.rest.repr
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class SerializerTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrependBaseUriToRelativePaths()
		 public virtual void ShouldPrependBaseUriToRelativePaths()
		 {
			  string baseUrl = "http://baseurl/";
			  Serializer serializer = new SerializerAnonymousInnerClass( this, URI.create( baseUrl ) );

			  string aRelativeUrl = "/path/path/path";
			  assertThat( serializer.RelativeUri( aRelativeUrl ), @is( baseUrl + aRelativeUrl.Substring( 1 ) ) );
			  assertThat( serializer.RelativeTemplate( aRelativeUrl ), @is( baseUrl + aRelativeUrl.Substring( 1 ) ) );
		 }

		 private class SerializerAnonymousInnerClass : Serializer
		 {
			 private readonly SerializerTest _outerInstance;

			 public SerializerAnonymousInnerClass( SerializerTest outerInstance, UnknownType create ) : base( create, null )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // empty
		 }

	}

}