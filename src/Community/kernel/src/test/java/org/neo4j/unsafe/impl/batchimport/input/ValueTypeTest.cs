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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ValueTypeTest
	{
		private bool InstanceFieldsInitialized = false;

		public ValueTypeTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void arraysShouldCalculateCorrectLength() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ArraysShouldCalculateCorrectLength()
		 {
			  // given
			  int[] value = new int[3];
			  for ( int i = 0; i < value.Length; i++ )
			  {
					value[i] = 100 + i;
			  }
			  ValueType valueType = ValueType.typeOf( value );
			  PhysicalFlushableChannel channel = new PhysicalFlushableChannel( Fs.open( Directory.file( "file" ), OpenMode.READ_WRITE ) );

			  // when
			  int length = valueType.Length( value );
			  valueType.Write( value, channel );

			  // then
			  int expected = 1 + Integer.BYTES + value.Length * Integer.BYTES; // array data
			  assertEquals( expected, length );
			  assertEquals( expected, channel.Position() );
		 }
	}

}