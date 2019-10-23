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
namespace Neo4Net.Kernel.impl.store.format
{
	/// <summary>
	/// All known store formats are collected here.
	/// </summary>
	public sealed class StoreVersion
	{
		 public static readonly StoreVersion StandardV2_3 = new StoreVersion( "StandardV2_3", InnerEnum.StandardV2_3, "v0.A.6", "2.3.0" );
		 public static readonly StoreVersion StandardV3_0 = new StoreVersion( "StandardV3_0", InnerEnum.StandardV3_0, "v0.A.7", "3.0.0" );
		 public static readonly StoreVersion StandardV3_2 = new StoreVersion( "StandardV3_2", InnerEnum.StandardV3_2, "v0.A.8", "3.2.0" );
		 public static readonly StoreVersion StandardV3_4 = new StoreVersion( "StandardV3_4", InnerEnum.StandardV3_4, "v0.A.9", "3.4.0" );

		 public static readonly StoreVersion HighLimitV3_0_0 = new StoreVersion( "HighLimitV3_0_0", InnerEnum.HighLimitV3_0_0, "vE.H.0", "3.0.0" );
		 public static readonly StoreVersion HighLimitV3_0_6 = new StoreVersion( "HighLimitV3_0_6", InnerEnum.HighLimitV3_0_6, "vE.H.0b", "3.0.6" );
		 public static readonly StoreVersion HighLimitV3_1_0 = new StoreVersion( "HighLimitV3_1_0", InnerEnum.HighLimitV3_1_0, "vE.H.2", "3.1.0" );
		 public static readonly StoreVersion HighLimitV3_2_0 = new StoreVersion( "HighLimitV3_2_0", InnerEnum.HighLimitV3_2_0, "vE.H.3", "3.2.0" );
		 public static readonly StoreVersion HighLimitV3_4_0 = new StoreVersion( "HighLimitV3_4_0", InnerEnum.HighLimitV3_4_0, "vE.H.4", "3.4.0" );

		 private static readonly IList<StoreVersion> valueList = new List<StoreVersion>();

		 static StoreVersion()
		 {
			 valueList.Add( StandardV2_3 );
			 valueList.Add( StandardV3_0 );
			 valueList.Add( StandardV3_2 );
			 valueList.Add( StandardV3_4 );
			 valueList.Add( HighLimitV3_0_0 );
			 valueList.Add( HighLimitV3_0_6 );
			 valueList.Add( HighLimitV3_1_0 );
			 valueList.Add( HighLimitV3_2_0 );
			 valueList.Add( HighLimitV3_4_0 );
		 }

		 public enum InnerEnum
		 {
			 StandardV2_3,
			 StandardV3_0,
			 StandardV3_2,
			 StandardV3_4,
			 HighLimitV3_0_0,
			 HighLimitV3_0_6,
			 HighLimitV3_1_0,
			 HighLimitV3_2_0,
			 HighLimitV3_4_0
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;

		 internal StoreVersion( string name, InnerEnum innerEnum, string versionString, string introductionVersion )
		 {
			  this._versionString = versionString;
			  this._introductionVersion = introductionVersion;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public string VersionString()
		 {
			  return _versionString;
		 }

		 public string IntroductionVersion()
		 {
			  return _introductionVersion;
		 }

		public static IList<StoreVersion> values()
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

		public static StoreVersion ValueOf( string name )
		{
			foreach ( StoreVersion enumInstance in StoreVersion.valueList )
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