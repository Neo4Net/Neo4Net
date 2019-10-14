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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.contains;

	/// <summary>
	/// A collection of high level capabilities a store can have, should not be more granular than necessary
	/// for differentiating different version from one another.
	/// </summary>
	public sealed class Capability
	{
		 /// <summary>
		 /// Store has schema support
		 /// </summary>
		 public static readonly Capability Schema = new Capability( "Schema", InnerEnum.Schema, CapabilityType.Store );

		 /// <summary>
		 /// Store has dense node support
		 /// </summary>
		 public static readonly Capability DenseNodes = new Capability( "DenseNodes", InnerEnum.DenseNodes, CapabilityType.Format, CapabilityType.Store );

		 /// <summary>
		 /// 3 bytes relationship type support
		 /// </summary>
		 public static readonly Capability RelationshipType_3bytes = new Capability( "RelationshipType_3bytes", InnerEnum.RelationshipType_3bytes, CapabilityType.Format, CapabilityType.Store );

		 /// <summary>
		 /// Lucene version 3.x
		 /// </summary>
		 public static readonly Capability Lucene_3 = new Capability( "Lucene_3", InnerEnum.Lucene_3, CapabilityType.Index );

		 /// <summary>
		 /// Lucene version 5.x
		 /// </summary>
		 public static readonly Capability Lucene_5 = new Capability( "Lucene_5", InnerEnum.Lucene_5, CapabilityType.Index );

		 /// <summary>
		 /// Point Geometries are an addition to the format, not a change
		 /// </summary>
		 public static readonly Capability PointProperties = new Capability( "PointProperties", InnerEnum.PointProperties, true, CapabilityType.Store );

		 /// <summary>
		 /// Temporal types are an addition to the format, not a change
		 /// </summary>
		 public static readonly Capability TemporalProperties = new Capability( "TemporalProperties", InnerEnum.TemporalProperties, true, CapabilityType.Store );

		 /// <summary>
		 /// Records can spill over into secondary units (another record with a header saying it's a secondary unit to another record).
		 /// </summary>
		 public static readonly Capability SecondaryRecordUnits = new Capability( "SecondaryRecordUnits", InnerEnum.SecondaryRecordUnits, CapabilityType.Format );

		 private static readonly IList<Capability> valueList = new List<Capability>();

		 static Capability()
		 {
			 valueList.Add( Schema );
			 valueList.Add( DenseNodes );
			 valueList.Add( RelationshipType_3bytes );
			 valueList.Add( Lucene_3 );
			 valueList.Add( Lucene_5 );
			 valueList.Add( PointProperties );
			 valueList.Add( TemporalProperties );
			 valueList.Add( SecondaryRecordUnits );
		 }

		 public enum InnerEnum
		 {
			 Schema,
			 DenseNodes,
			 RelationshipType_3bytes,
			 Lucene_3,
			 Lucene_5,
			 PointProperties,
			 TemporalProperties,
			 SecondaryRecordUnits
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private boolean;

		 internal Capability( string name, InnerEnum innerEnum, params CapabilityType[] types ) : this( false, types )
		 {

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal Capability( string name, InnerEnum innerEnum, bool additive, params CapabilityType[] types )
		 {
			  this._additive = additive;
			  this._types = types;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public bool IsType( CapabilityType type )
		 {
			  return contains( _types, type );
		 }

		 /// <summary>
		 /// Whether or not this capability is additive. A capability is additive if data regarding this capability will not change
		 /// any existing store and therefore not require migration of existing data.
		 /// </summary>
		 /// <returns> whether or not this capability is additive. </returns>
		 public bool Additive
		 {
			 get
			 {
				  return _additive;
			 }
		 }

		public static IList<Capability> values()
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

		public static Capability valueOf( string name )
		{
			foreach ( Capability enumInstance in Capability.valueList )
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