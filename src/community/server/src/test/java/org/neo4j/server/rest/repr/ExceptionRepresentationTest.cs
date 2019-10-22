using System;
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
namespace Neo4Net.Server.rest.repr
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Test = org.junit.Test;


	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using MapWrappingWriter = Neo4Net.Server.rest.repr.formats.MapWrappingWriter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.exceptions.Status_General.UnknownError;

	public class ExceptionRepresentationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeCause() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeCause()
		 {
			  // Given
			  ExceptionRepresentation rep = new ExceptionRepresentation( new Exception( "Hoho", new Exception( "Haha", new Exception( "HAHA!" ) ) ) );

			  // When
			  JsonNode @out = Serialize( rep );

			  // Then
			  assertThat( @out.get( "cause" ).get( "message" ).asText(), @is("Haha") );
			  assertThat( @out.get( "cause" ).get( "cause" ).get( "message" ).asText(), @is("HAHA!") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRenderErrorsWithNeo4NetStatusCode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRenderErrorsWithNeo4NetStatusCode()
		 {
			  // Given
			  ExceptionRepresentation rep = new ExceptionRepresentation( new KernelExceptionAnonymousInnerClass( this, UnknownError ) );

			  // When
			  JsonNode @out = Serialize( rep );

			  // Then
			  assertThat( @out.get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.DatabaseError.General.UnknownError") );
			  assertThat( @out.get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("Hello") );
		 }

		 private class KernelExceptionAnonymousInnerClass : KernelException
		 {
			 private readonly ExceptionRepresentationTest _outerInstance;

			 public KernelExceptionAnonymousInnerClass( ExceptionRepresentationTest outerInstance, UnknownType unknownError ) : base( unknownError, "Hello" )
			 {
				 this.outerInstance = outerInstance;
			 }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExcludeLegacyFormatIfAsked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExcludeLegacyFormatIfAsked()
		 {
			  // Given
			  ExceptionRepresentation rep = new ExceptionRepresentation(new KernelExceptionAnonymousInnerClass2(this, UnknownError)
			 , false);

			  // When
			  JsonNode @out = Serialize( rep );

			  // Then
			  assertThat( @out.get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.DatabaseError.General.UnknownError") );
			  assertThat( @out.get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("Hello") );
			  assertThat( @out.has( "message" ), equalTo( false ) );
		 }

		 private class KernelExceptionAnonymousInnerClass2 : KernelException
		 {
			 private readonly ExceptionRepresentationTest _outerInstance;

			 public KernelExceptionAnonymousInnerClass2( ExceptionRepresentationTest outerInstance, UnknownType unknownError ) : base( unknownError, "Hello" )
			 {
				 this.outerInstance = outerInstance;
			 }

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.codehaus.jackson.JsonNode serialize(ExceptionRepresentation rep) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private JsonNode Serialize( ExceptionRepresentation rep )
		 {
			  IDictionary<string, object> output = new Dictionary<string, object>();
			  MappingSerializer serializer = new MappingSerializer( new MapWrappingWriter( output ), URI.create( "" ), mock( typeof( ExtensionInjector ) ) );

			  // When
			  rep.Serialize( serializer );
			  return JsonHelper.jsonNode( JsonHelper.createJsonFrom( output ) );
		 }
	}

}