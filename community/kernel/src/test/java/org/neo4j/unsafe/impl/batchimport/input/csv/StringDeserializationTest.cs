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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input.csv
{
	using Test = org.junit.Test;

	using Extractors = Org.Neo4j.Csv.Reader.Extractors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class StringDeserializationTest
	{
		private bool InstanceFieldsInitialized = false;

		public StringDeserializationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_extractors = new Extractors( _configuration.arrayDelimiter() );
			_entry1 = new Header.Entry( null, Type.StartId, null, _extractors.int_() );
			_entry2 = new Header.Entry( null, Type.Type, null, _extractors.@string() );
			_entry3 = new Header.Entry( null, Type.EndId, null, _extractors.int_() );
		}

		 private readonly Configuration _configuration = Configuration.COMMAS;
		 private Extractors _extractors;
		 private Header.Entry _entry1;
		 private Header.Entry _entry2;
		 private Header.Entry _entry3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideDelimiterAfterFirstEmptyField()
		 public virtual void ShouldProvideDelimiterAfterFirstEmptyField()
		 {
			  // given
			  StringDeserialization deserialization = new StringDeserialization( _configuration );

			  // when
			  deserialization.Handle( _entry1, null );
			  deserialization.Handle( _entry2, "MyType" );
			  deserialization.Handle( _entry3, 123 );
			  string line = deserialization.Materialize();

			  // then
			  assertEquals( line, ",MyType,123" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideDelimiterBeforeLastEmptyField()
		 public virtual void ShouldProvideDelimiterBeforeLastEmptyField()
		 {
			  // given
			  StringDeserialization deserialization = new StringDeserialization( _configuration );

			  // when
			  deserialization.Handle( _entry1, 123 );
			  deserialization.Handle( _entry2, "MyType" );
			  deserialization.Handle( _entry3, null );
			  string line = deserialization.Materialize();

			  // then
			  assertEquals( line, "123,MyType," );
		 }
	}

}