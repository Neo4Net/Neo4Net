using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.protocol
{

	public interface Protocol<IMPL> where IMPL : IComparable<IMPL>
	{
		 string Category();

		 IMPL Implementation();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <IMPL, T> java.util.Optional<T> find(T[] values, Protocol_Category<T> category, IMPL implementation, System.Func<IMPL, IMPL> normalise)
	//	 {
	//		  return Stream.of(values).filter(protocol -> Objects.equals(protocol.category(), category.canonicalName())).filter(protocol -> Objects.equals(normalise.apply(protocol.implementation()), normalise.apply(implementation))).findFirst();
	//	 }
	}

	 public interface Protocol_Category<T> where T : Protocol
	 {
		  string CanonicalName();
	 }

	 public interface Protocol_ApplicationProtocol : Protocol<int>
	 {
	 }

	 public sealed class Protocol_ApplicationProtocolCategory : Protocol_Category<Protocol_ApplicationProtocol>
	 {
		  public static readonly Protocol_ApplicationProtocolCategory Raft = new Protocol_ApplicationProtocolCategory( "Raft", InnerEnum.Raft );
		  public static readonly Protocol_ApplicationProtocolCategory Catchup = new Protocol_ApplicationProtocolCategory( "Catchup", InnerEnum.Catchup );

		  private static readonly IList<Protocol_ApplicationProtocolCategory> valueList = new List<Protocol_ApplicationProtocolCategory>();

		  static Protocol_ApplicationProtocolCategory()
		  {
			  valueList.Add( Raft );
			  valueList.Add( Catchup );
		  }

		  public enum InnerEnum
		  {
			  Raft,
			  Catchup
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  private Protocol_ApplicationProtocolCategory( string name, InnerEnum innerEnum )
		  {
			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string CanonicalName()
		  {
				return name().ToLower();
		  }

		 public static IList<Protocol_ApplicationProtocolCategory> values()
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

		 public static Protocol_ApplicationProtocolCategory valueOf( string name )
		 {
			 foreach ( Protocol_ApplicationProtocolCategory enumInstance in Protocol_ApplicationProtocolCategory.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Protocol_ApplicationProtocols : Protocol_ApplicationProtocol
	 {
		  public static readonly Protocol_ApplicationProtocols Raft_1 = new Protocol_ApplicationProtocols( "Raft_1", InnerEnum.Raft_1, Protocol_ApplicationProtocolCategory.Raft, 1 );
		  public static readonly Protocol_ApplicationProtocols Raft_2 = new Protocol_ApplicationProtocols( "Raft_2", InnerEnum.Raft_2, Protocol_ApplicationProtocolCategory.Raft, 2 );
		  public static readonly Protocol_ApplicationProtocols Catchup_1 = new Protocol_ApplicationProtocols( "Catchup_1", InnerEnum.Catchup_1, Protocol_ApplicationProtocolCategory.Catchup, 1 );

		  private static readonly IList<Protocol_ApplicationProtocols> valueList = new List<Protocol_ApplicationProtocols>();

		  static Protocol_ApplicationProtocols()
		  {
			  valueList.Add( Raft_1 );
			  valueList.Add( Raft_2 );
			  valueList.Add( Catchup_1 );
		  }

		  public enum InnerEnum
		  {
			  Raft_1,
			  Raft_2,
			  Catchup_1
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final System;
		  internal Final Protocol_ApplicationProtocolCategory;

		  internal Protocol_ApplicationProtocols( string name, InnerEnum innerEnum, Protocol_ApplicationProtocolCategory identifier, int version )
		  {
				this.Identifier = identifier;
				this.Version = version;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string Category()
		  {
				return Identifier.canonicalName();
		  }

		  public int? Implementation()
		  {
				return Version;
		  }

		  public static Optional<Protocol_ApplicationProtocol> Find( Protocol_ApplicationProtocolCategory category, int? version )
		  {
				return Protocol.find( Protocol_ApplicationProtocols.values(), category, version, System.Func.identity() );
		  }

		 public static IList<Protocol_ApplicationProtocols> values()
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

		 public static Protocol_ApplicationProtocols valueOf( string name )
		 {
			 foreach ( Protocol_ApplicationProtocols enumInstance in Protocol_ApplicationProtocols.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public interface Protocol_ModifierProtocol : Protocol<string>
	 {
	 }

	 public sealed class Protocol_ModifierProtocolCategory : Protocol_Category<Protocol_ModifierProtocol>
	 {
		  public static readonly Protocol_ModifierProtocolCategory Compression = new Protocol_ModifierProtocolCategory( "Compression", InnerEnum.Compression );
		  // Need a second Category for testing purposes.
		  public static readonly Protocol_ModifierProtocolCategory GratuitousObfuscation = new Protocol_ModifierProtocolCategory( "GratuitousObfuscation", InnerEnum.GratuitousObfuscation );

		  private static readonly IList<Protocol_ModifierProtocolCategory> valueList = new List<Protocol_ModifierProtocolCategory>();

		  static Protocol_ModifierProtocolCategory()
		  {
			  valueList.Add( Compression );
			  valueList.Add( GratuitousObfuscation );
		  }

		  public enum InnerEnum
		  {
			  Compression,
			  GratuitousObfuscation
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  private Protocol_ModifierProtocolCategory( string name, InnerEnum innerEnum )
		  {
			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string CanonicalName()
		  {
				return name().ToLower();
		  }

		 public static IList<Protocol_ModifierProtocolCategory> values()
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

		 public static Protocol_ModifierProtocolCategory valueOf( string name )
		 {
			 foreach ( Protocol_ModifierProtocolCategory enumInstance in Protocol_ModifierProtocolCategory.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public sealed class Protocol_ModifierProtocols : Protocol_ModifierProtocol
	 {
		  public static readonly Protocol_ModifierProtocols CompressionGzip = new Protocol_ModifierProtocols( "CompressionGzip", InnerEnum.CompressionGzip, Protocol_ModifierProtocolCategory.Compression, Implementations.GZIP );
		  public static readonly Protocol_ModifierProtocols CompressionSnappy = new Protocol_ModifierProtocols( "CompressionSnappy", InnerEnum.CompressionSnappy, Protocol_ModifierProtocolCategory.Compression, Implementations.SNAPPY );
		  public static readonly Protocol_ModifierProtocols CompressionSnappyValidating = new Protocol_ModifierProtocols( "CompressionSnappyValidating", InnerEnum.CompressionSnappyValidating, Protocol_ModifierProtocolCategory.Compression, Implementations.SNAPPY_VALIDATING );
		  public static readonly Protocol_ModifierProtocols CompressionLz4 = new Protocol_ModifierProtocols( "CompressionLz4", InnerEnum.CompressionLz4, Protocol_ModifierProtocolCategory.Compression, Implementations.LZ4 );
		  public static readonly Protocol_ModifierProtocols CompressionLz4HighCompression = new Protocol_ModifierProtocols( "CompressionLz4HighCompression", InnerEnum.CompressionLz4HighCompression, Protocol_ModifierProtocolCategory.Compression, Implementations.LZ4_HIGH_COMPRESSION );
		  public static readonly Protocol_ModifierProtocols CompressionLz4Validating = new Protocol_ModifierProtocols( "CompressionLz4Validating", InnerEnum.CompressionLz4Validating, Protocol_ModifierProtocolCategory.Compression, Implementations.LZ_VALIDATING );
		  public static readonly Protocol_ModifierProtocols CompressionLz4HighCompressionValidating = new Protocol_ModifierProtocols( "CompressionLz4HighCompressionValidating", InnerEnum.CompressionLz4HighCompressionValidating, Protocol_ModifierProtocolCategory.Compression, Implementations.LZ4_HIGH_COMPRESSION_VALIDATING );

		  private static readonly IList<Protocol_ModifierProtocols> valueList = new List<Protocol_ModifierProtocols>();

		  static Protocol_ModifierProtocols()
		  {
			  valueList.Add( CompressionGzip );
			  valueList.Add( CompressionSnappy );
			  valueList.Add( CompressionSnappyValidating );
			  valueList.Add( CompressionLz4 );
			  valueList.Add( CompressionLz4HighCompression );
			  valueList.Add( CompressionLz4Validating );
			  valueList.Add( CompressionLz4HighCompressionValidating );
		  }

		  public enum InnerEnum
		  {
			  CompressionGzip,
			  CompressionSnappy,
			  CompressionSnappyValidating,
			  CompressionLz4,
			  CompressionLz4HighCompression,
			  CompressionLz4Validating,
			  CompressionLz4HighCompressionValidating
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  // Should be human writable into a comma separated list
		  internal Final String;
		  internal Final Protocol_ModifierProtocolCategory;

		  internal Protocol_ModifierProtocols( string name, InnerEnum innerEnum, Protocol_ModifierProtocolCategory identifier, string friendlyName )
		  {
				this.Identifier = identifier;
				this.FriendlyName = friendlyName;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public string Implementation()
		  {
				return FriendlyName;
		  }

		  public string Category()
		  {
				return Identifier.canonicalName();
		  }

		  public static Optional<Protocol_ModifierProtocol> Find( Protocol_ModifierProtocolCategory category, string friendlyName )
		  {
				return Protocol.find( Protocol_ModifierProtocols.values(), category, friendlyName, string.ToLower );
		  }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		  public static class Implementations
	//	  {
	//			public static final String GZIP = "Gzip";
	//			public static final String SNAPPY = "Snappy";
	//			public static final String SNAPPY_VALIDATING = "Snappy_validating";
	//			public static final String LZ4 = "LZ4";
	//			public static final String LZ4_HIGH_COMPRESSION = "LZ4_high_compression";
	//			public static final String LZ_VALIDATING = "LZ_validating";
	//			public static final String LZ4_HIGH_COMPRESSION_VALIDATING = "LZ4_high_compression_validating";
	//	  }

		 public static IList<Protocol_ModifierProtocols> values()
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

		 public static Protocol_ModifierProtocols valueOf( string name )
		 {
			 foreach ( Protocol_ModifierProtocols enumInstance in Protocol_ModifierProtocols.valueList )
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