using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.protocol.handshake
{

	using Neo4Net.causalclustering.protocol;

	public interface TestProtocols
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <U, T> T latest(Neo4Net.causalclustering.protocol.Protocol_Category<T> category, T[] values)
	//	 {
	//		  return Stream.of(values).filter(protocol -> protocol.category().equals(category.canonicalName())).max(Comparator.comparing(T::implementation)).get();
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <U, T> U[] allVersionsOf(Neo4Net.causalclustering.protocol.Protocol_Category<T> category, T[] values, System.Func<int, U[]> constructor)
	//	 {
	//		  return Stream.of(values).filter(protocol -> protocol.category().equals(category.canonicalName())).map(Protocol::implementation).toArray(constructor);
	//	 }
	}

	 public sealed class TestProtocols_TestApplicationProtocols : Protocol_ApplicationProtocol
	 {
		  public static readonly TestProtocols_TestApplicationProtocols Raft_1 = new TestProtocols_TestApplicationProtocols( "Raft_1", InnerEnum.Raft_1, ApplicationProtocolCategory.RAFT, 1 );
		  public static readonly TestProtocols_TestApplicationProtocols Raft_2 = new TestProtocols_TestApplicationProtocols( "Raft_2", InnerEnum.Raft_2, ApplicationProtocolCategory.RAFT, 2 );
		  public static readonly TestProtocols_TestApplicationProtocols Raft_3 = new TestProtocols_TestApplicationProtocols( "Raft_3", InnerEnum.Raft_3, ApplicationProtocolCategory.RAFT, 3 );
		  public static readonly TestProtocols_TestApplicationProtocols Raft_4 = new TestProtocols_TestApplicationProtocols( "Raft_4", InnerEnum.Raft_4, ApplicationProtocolCategory.RAFT, 4 );
		  public static readonly TestProtocols_TestApplicationProtocols Catchup_1 = new TestProtocols_TestApplicationProtocols( "Catchup_1", InnerEnum.Catchup_1, ApplicationProtocolCategory.CATCHUP, 1 );
		  public static readonly TestProtocols_TestApplicationProtocols Catchup_2 = new TestProtocols_TestApplicationProtocols( "Catchup_2", InnerEnum.Catchup_2, ApplicationProtocolCategory.CATCHUP, 2 );
		  public static readonly TestProtocols_TestApplicationProtocols Catchup_3 = new TestProtocols_TestApplicationProtocols( "Catchup_3", InnerEnum.Catchup_3, ApplicationProtocolCategory.CATCHUP, 3 );
		  public static readonly TestProtocols_TestApplicationProtocols Catchup_4 = new TestProtocols_TestApplicationProtocols( "Catchup_4", InnerEnum.Catchup_4, ApplicationProtocolCategory.CATCHUP, 4 );

		  private static readonly IList<TestProtocols_TestApplicationProtocols> valueList = new List<TestProtocols_TestApplicationProtocols>();

		  static TestProtocols_TestApplicationProtocols()
		  {
			  valueList.Add( Raft_1 );
			  valueList.Add( Raft_2 );
			  valueList.Add( Raft_3 );
			  valueList.Add( Raft_4 );
			  valueList.Add( Catchup_1 );
			  valueList.Add( Catchup_2 );
			  valueList.Add( Catchup_3 );
			  valueList.Add( Catchup_4 );
		  }

		  public enum InnerEnum
		  {
			  Raft_1,
			  Raft_2,
			  Raft_3,
			  Raft_4,
			  Catchup_1,
			  Catchup_2,
			  Catchup_3,
			  Catchup_4
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final System;

		  internal Final ApplicationProtocolCategory;
		  internal TestProtocols_TestApplicationProtocols( string name, InnerEnum innerEnum, ApplicationProtocolCategory identifier, int version )
		  {
				this.Identifier = identifier;
				this.Version = version;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string Category()
		  {
				return this.Identifier.canonicalName();
		  }

		  public int? Implementation()
		  {
				return Version;
		  }

		  public static ApplicationProtocol Latest( ApplicationProtocolCategory identifier )
		  {
				return TestProtocols.latest( identifier, values() );
		  }

		  public static int?[] AllVersionsOf( ApplicationProtocolCategory identifier )
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				return TestProtocols.allVersionsOf( identifier, TestProtocols_TestApplicationProtocols.values(), int?[]::new );
		  }

		  public static IList<int> ListVersionsOf( ApplicationProtocolCategory identifier )
		  {
				return Arrays.asList( AllVersionsOf( identifier ) );
		  }

		 public static IList<TestProtocols_TestApplicationProtocols> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static TestProtocols_TestApplicationProtocols ValueOf( string name )
		 {
			 foreach ( TestProtocols_TestApplicationProtocols enumInstance in TestProtocols_TestApplicationProtocols.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class TestProtocols_TestModifierProtocols : Protocol_ModifierProtocol
	 {
		  public static readonly TestProtocols_TestModifierProtocols Snappy = new TestProtocols_TestModifierProtocols( "Snappy", InnerEnum.Snappy, ModifierProtocolCategory.COMPRESSION, "TestSnappy" );
		  public static readonly TestProtocols_TestModifierProtocols Lzo = new TestProtocols_TestModifierProtocols( "Lzo", InnerEnum.Lzo, ModifierProtocolCategory.COMPRESSION, "TestLZO" );
		  public static readonly TestProtocols_TestModifierProtocols Lz4 = new TestProtocols_TestModifierProtocols( "Lz4", InnerEnum.Lz4, ModifierProtocolCategory.COMPRESSION, "TestLZ4" );
		  public static readonly TestProtocols_TestModifierProtocols Lz4Validating = new TestProtocols_TestModifierProtocols( "Lz4Validating", InnerEnum.Lz4Validating, ModifierProtocolCategory.COMPRESSION, "TestLZ4Validating" );
		  public static readonly TestProtocols_TestModifierProtocols Lz4HighCompression = new TestProtocols_TestModifierProtocols( "Lz4HighCompression", InnerEnum.Lz4HighCompression, ModifierProtocolCategory.COMPRESSION, "TestLZ4High" );
		  public static readonly TestProtocols_TestModifierProtocols Lz4HighCompressionValidating = new TestProtocols_TestModifierProtocols( "Lz4HighCompressionValidating", InnerEnum.Lz4HighCompressionValidating, ModifierProtocolCategory.COMPRESSION, "TestLZ4HighValidating" );
		  public static readonly TestProtocols_TestModifierProtocols Rot13 = new TestProtocols_TestModifierProtocols( "Rot13", InnerEnum.Rot13, ModifierProtocolCategory.GRATUITOUS_OBFUSCATION, "ROT13" );
		  public static readonly TestProtocols_TestModifierProtocols NameClash = new TestProtocols_TestModifierProtocols( "NameClash", InnerEnum.NameClash, ModifierProtocolCategory.GRATUITOUS_OBFUSCATION, "TestSnappy" );

		  private static readonly IList<TestProtocols_TestModifierProtocols> valueList = new List<TestProtocols_TestModifierProtocols>();

		  static TestProtocols_TestModifierProtocols()
		  {
			  valueList.Add( Snappy );
			  valueList.Add( Lzo );
			  valueList.Add( Lz4 );
			  valueList.Add( Lz4Validating );
			  valueList.Add( Lz4HighCompression );
			  valueList.Add( Lz4HighCompressionValidating );
			  valueList.Add( Rot13 );
			  valueList.Add( NameClash );
		  }

		  public enum InnerEnum
		  {
			  Snappy,
			  Lzo,
			  Lz4,
			  Lz4Validating,
			  Lz4HighCompression,
			  Lz4HighCompressionValidating,
			  Rot13,
			  NameClash
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final ModifierProtocolCategory;
		  internal Final String;

		  internal TestProtocols_TestModifierProtocols( string name, InnerEnum innerEnum, ModifierProtocolCategory identifier, string friendlyName )
		  {
				this.Identifier = identifier;
				this.FriendlyName = friendlyName;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string Category()
		  {
				return Identifier.canonicalName();
		  }

		  public string Implementation()
		  {
				return FriendlyName;
		  }

		  public static ModifierProtocol Latest( ModifierProtocolCategory identifier )
		  {
				return TestProtocols.latest( identifier, values() );
		  }

		  public static string[] AllVersionsOf( ModifierProtocolCategory identifier )
		  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				return TestProtocols.allVersionsOf( identifier, TestProtocols_TestModifierProtocols.values(), string[]::new );
		  }

		  public static IList<string> ListVersionsOf( ModifierProtocolCategory identifier )
		  {
				IList<string> versions = Arrays.asList( AllVersionsOf( identifier ) );
				versions.sort( System.Collections.IComparer.reverseOrder() );
				return versions;
		  }

		 public static IList<TestProtocols_TestModifierProtocols> values()
		 {
			 return valueList;
		 }

		 public int ordinal()
		 {
			 return ordinalValue;
		 }

		 public override string ToString()
		 {
			 return nameValue;
		 }

		 public static TestProtocols_TestModifierProtocols ValueOf( string name )
		 {
			 foreach ( TestProtocols_TestModifierProtocols enumInstance in TestProtocols_TestModifierProtocols.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

}