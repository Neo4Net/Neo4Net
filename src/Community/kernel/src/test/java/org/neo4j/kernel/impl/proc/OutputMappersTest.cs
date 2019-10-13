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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using OutputMapper = Neo4Net.Kernel.impl.proc.OutputMappers.OutputMapper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.FieldSignature.outputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTString;

	public class OutputMappersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 public class SingleStringFieldRecord
		 {
			  public string Name;

			  public SingleStringFieldRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class UnmappableRecord
		 {
			  public UnmappableRecord Wat;
		 }

		 public class RecordWithPrivateField
		 {
			  internal string Wat;
		 }

		 public class RecordWithNonStringKeyMap
		 {
			  public IDictionary<RecordWithNonStringKeyMap, object> Wat;
		 }

		 public class RecordWithStaticFields
		 {
			  public static string SkipMePublic;
			  public string IncludeMe;
			  internal static string SkipMePrivate;

			  public RecordWithStaticFields( string val )
			  {
					this.IncludeMe = val;
			  }
		 }

		 public class RecordWithDeprecatedFields
		 {
			  [Obsolete]
			  public string Deprecated;
			  public string Replacement;
			  [Obsolete]
			  public string AlsoDeprecated;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapSimpleRecordWithString() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMapSimpleRecordWithString()
		 {
			  // When
			  OutputMapper mapper = mapper( typeof( SingleStringFieldRecord ) );

			  // Then
			  assertThat( mapper.Signature(), contains(outputField("name", NTString)) );
			  assertThat( asList( mapper.Apply( new SingleStringFieldRecord( "hello, world!" ) ) ), contains( "hello, world!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipStaticFields() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipStaticFields()
		 {
			  // When
			  OutputMapper mapper = mapper( typeof( RecordWithStaticFields ) );

			  // Then
			  assertThat( mapper.Signature(), contains(outputField("includeMe", NTString)) );
			  assertThat( asList( mapper.Apply( new RecordWithStaticFields( "hello, world!" ) ) ), contains( "hello, world!" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNoteDeprecatedFields() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNoteDeprecatedFields()
		 {
			  // when
			  OutputMapper mapper = mapper( typeof( RecordWithDeprecatedFields ) );

			  // then
			  assertThat( mapper.Signature(), containsInAnyOrder(outputField("deprecated", NTString, true), outputField("alsoDeprecated", NTString, true), outputField("replacement", NTString, false)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnUnmappable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnUnmappable()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Field `wat` in record `UnmappableRecord` cannot be converted to a Neo4j type:" + " Don't know how to map `org.neo4j.kernel.impl.proc.OutputMappersTest$UnmappableRecord`" );

			  // When
			  Mapper( typeof( UnmappableRecord ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnPrivateField() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnPrivateField()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Field `wat` in record `RecordWithPrivateField` cannot be accessed. " + "Please ensure the field is marked as `public`." );

			  // When
			  Mapper( typeof( RecordWithPrivateField ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnMapWithNonStringKeyMap() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveHelpfulErrorOnMapWithNonStringKeyMap()
		 {
			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( "Field `wat` in record `RecordWithNonStringKeyMap` cannot be converted " + "to a Neo4j type: Maps are required to have `String` keys - but this map " + "has `org.neo4j.kernel.impl.proc.OutputMappersTest$RecordWithNonStringKeyMap` keys." );

			  // When
			  Mapper( typeof( RecordWithNonStringKeyMap ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAgainstStdLibraryClassesSinceTheseIndicateUserError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWarnAgainstStdLibraryClassesSinceTheseIndicateUserError()
		 {
			  // Impl note: We may want to change this behavior and actually allow procedures to return `Long` etc,
			  //            with a default column name. So Stream<Long> would become records like (out: Long)
			  //            Drawback of that is that it'd cause cognitive dissonance, it's not obvious what's a record
			  //            and what is a primitive value..

			  // Expect
			  Exception.expect( typeof( ProcedureException ) );
			  Exception.expectMessage( string.Format( "Procedures must return a Stream of records, where a record is a concrete class%n" + "that you define, with public non-final fields defining the fields in the record.%n" + "If you''d like your procedure to return `Long`, you could define a record class like:%n" + "public class Output '{'%n" + "    public Long out;%n" + "'}'%n" + "%n" + "And then define your procedure as returning `Stream<Output>`." ) );

			  // When
			  Mapper( typeof( Long ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.proc.OutputMappers.OutputMapper mapper(Class clazz) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private OutputMapper Mapper( Type clazz )
		 {
			  return ( new OutputMappers( new TypeMappers() ) ).Mapper(clazz);
		 }

	}

}