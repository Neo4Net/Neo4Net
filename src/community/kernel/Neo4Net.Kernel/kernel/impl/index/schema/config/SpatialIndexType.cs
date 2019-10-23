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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{

	using Envelope = Neo4Net.Gis.Spatial.Index.Envelope;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	internal abstract class SpatialIndexType
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SingleSpaceFillingCurve(1) { public void writeHeader(SpaceFillingCurveSettings settings, org.Neo4Net.io.pagecache.PageCursor cursor) { cursor.putInt(settings.maxLevels); cursor.putInt(settings.dimensions); double[] min = settings.extents.getMin(); double[] max = settings.extents.getMax(); for(int i = 0; i < settings.dimensions; i++) { cursor.putLong(Double.doubleToLongBits(min[i])); cursor.putLong(Double.doubleToLongBits(max[i])); } } public void readHeader(SpaceFillingCurveSettings.SettingsFromIndexHeader settings, ByteBuffer headerBytes) { try { settings.maxLevels = headerBytes.getInt(); settings.dimensions = headerBytes.getInt(); double[] min = new double[settings.dimensions]; double[] max = new double[settings.dimensions]; for(int i = 0; i < settings.dimensions; i++) { min[i] = headerBytes.getDouble(); max[i] = headerBytes.getDouble(); } settings.extents = new org.Neo4Net.gis.spatial.index.Envelope(min, max); } catch(java.nio.BufferUnderflowException e) { settings.markAsFailed("Failed to read settings from GBPTree header: " + e.getMessage()); } } };

		 private static readonly IList<SpatialIndexType> valueList = new List<SpatialIndexType>();

		 static SpatialIndexType()
		 {
			 valueList.Add( SingleSpaceFillingCurve );
		 }

		 public enum InnerEnum
		 {
			 SingleSpaceFillingCurve
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private SpatialIndexType( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }
		 internal int id;

		 public abstract void writeHeader( SpaceFillingCurveSettings settings, Neo4Net.Io.pagecache.PageCursor cursor );

		 public abstract void readHeader( SpaceFillingCurveSettings.SettingsFromIndexHeader settingsFromIndexHeader, ByteBuffer headerBytes );

		 public static readonly SpatialIndexType SpatialIndexType( int id ) { this.id = id; } static SpatialIndexType get( int id )
		 {
			 for ( SpatialIndexType type : values() )
			 {
				 if ( type.id == id ) { return type; }
			 }
			 return null;
		 }
		 = new SpatialIndexType("SpatialIndexType(int id) { this.id = id; } static SpatialIndexType get(int id) { for(SpatialIndexType type : values()) { if(type.id == id) { return type; } } return null; }", InnerEnum.SpatialIndexType(int id) { this.id = id; } static SpatialIndexType get(int id)
		 {
			 for ( SpatialIndexType type : values() )
			 {
				 if ( type.id == id ) { return type; }
			 }
			 return null;
		 });

		public static IList<SpatialIndexType> values()
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

		public static SpatialIndexType ValueOf( string name )
		{
			foreach ( SpatialIndexType enumInstance in SpatialIndexType.valueList )
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