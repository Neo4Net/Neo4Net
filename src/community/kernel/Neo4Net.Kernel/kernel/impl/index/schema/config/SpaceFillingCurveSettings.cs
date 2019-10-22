using System;

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
	using HilbertSpaceFillingCurve2D = Neo4Net.Gis.Spatial.Index.curves.HilbertSpaceFillingCurve2D;
	using HilbertSpaceFillingCurve3D = Neo4Net.Gis.Spatial.Index.curves.HilbertSpaceFillingCurve3D;
	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using Header = Neo4Net.Index.Internal.gbptree.Header;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.NativeIndexPopulator.BYTE_FAILED;

	/// <summary>
	/// <para>
	/// These settings affect the creation of the 2D (or 3D) to 1D mapper.
	/// Changing these will change the values of the 1D mapping, and require re-indexing, so
	/// once data has been indexed, do not change these without recreating the index.
	/// </para>
	/// <para>
	/// Key data maintained by this class include:
	/// <dl>
	///     <dt>dimensions</dt>
	///         <dd>either 2 or 3 for 2D or 3D</dd>
	///     <dt>maxLevels<dt>
	///         <dd>the number of levels in the tree that models the 2D to 1D mapper calculated as maxBits / dimensions</dd>
	///     <dt>extents</dt>
	///         <dd>The space filling curve is configured up front to cover a specific region of 2D (or 3D) space.
	/// Any points outside this space will be mapped as if on the edges. This means that if these extents
	/// do not match the real extents of the data being indexed, the index will be less efficient. Making
	/// the extents too big means than only a small area is used causing more points to map to fewer 1D
	/// values and requiring more post filtering. If the extents are too small, many points will lie on
	/// the edges, and also cause additional post-index filtering costs.</dd>
	/// </dl>
	/// </para>
	/// <para>If the settings are for an existing index, they are read from the GBPTree header, and in that case
	/// an additional field is maintained:
	/// <dl>
	///     <dt>failureMessage</dt>
	///         <dd>The settings are read from the GBPTree header structure, but when this is a FAILED index, there are no settings,
	///         but instead an error message describing the failure. If that happens, code that triggered the read should check this
	///         field and react accordingly. If the the value is null, there was no failure.</dd>
	/// </dl>
	/// </para>
	/// </summary>
	public class SpaceFillingCurveSettings
	{
		 private SpatialIndexType _indexType = SpatialIndexType.SingleSpaceFillingCurve;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal int DimensionsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal int MaxLevelsConflict;
		 internal Envelope Extents;

		 internal SpaceFillingCurveSettings( int dimensions, Envelope extents, int maxLevels )
		 {
			  this.DimensionsConflict = dimensions;
			  this.Extents = extents;
			  this.MaxLevelsConflict = maxLevels;
		 }

		 public virtual System.Action<PageCursor> HeaderWriter( sbyte initialIndexState )
		 {
			  return cursor =>
			  {
				cursor.putByte( initialIndexState );
				cursor.putInt( _indexType.id );
				_indexType.writeHeader( this, cursor );
			  };
		 }

		 /// <returns> The number of dimensions (2D or 3D) </returns>
		 public virtual int Dimensions
		 {
			 get
			 {
				  return DimensionsConflict;
			 }
		 }

		 /// <returns> The number of levels in the 2D (or 3D) to 1D mapping tree. </returns>
		 public virtual int MaxLevels
		 {
			 get
			 {
				  return MaxLevelsConflict;
			 }
		 }

		 /// <summary>
		 /// The space filling curve is configured up front to cover a specific region of 2D (or 3D) space.
		 /// Any points outside this space will be mapped as if on the edges. This means that if these extents
		 /// do not match the real extents of the data being indexed, the index will be less efficient. Making
		 /// the extents too big means than only a small area is used causing more points to map to fewer 1D
		 /// values and requiring more post filtering. If the extents are too small, many points will lie on
		 /// the edges, and also cause additional post-index filtering costs.
		 /// </summary>
		 /// <returns> the extents of the 2D (or 3D) region that is covered by the space filling curve. </returns>
		 public virtual Envelope IndexExtents()
		 {
			  return Extents;
		 }

		 /// <summary>
		 /// Make an instance of the SpaceFillingCurve that can perform the 2D (or 3D) to 1D mapping based on these settings.
		 /// </summary>
		 /// <returns> a configured instance of SpaceFillingCurve </returns>
		 public virtual SpaceFillingCurve Curve()
		 {
			  if ( DimensionsConflict == 2 )
			  {
					return new HilbertSpaceFillingCurve2D( Extents, MaxLevelsConflict );
			  }
			  else if ( DimensionsConflict == 3 )
			  {
					return new HilbertSpaceFillingCurve3D( Extents, MaxLevelsConflict );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot create spatial index with other than 2D or 3D coordinate reference system: " + DimensionsConflict + "D" );
			  }
		 }

		 public override int GetHashCode()
		 {
			  // dimension is also represented in the extents and so not explicitly included here
			  return 31 * Extents.GetHashCode() + MaxLevelsConflict;
		 }

		 public virtual bool Equals( SpaceFillingCurveSettings other )
		 {
			  return this.DimensionsConflict == other.DimensionsConflict && this.MaxLevelsConflict == other.MaxLevelsConflict && this.Extents.Equals( other.Extents );
		 }

		 public override bool Equals( object obj )
		 {
			  return obj is SpaceFillingCurveSettings && Equals( ( SpaceFillingCurveSettings ) obj );
		 }

		 public override string ToString()
		 {
			  return string.Format( "Space filling curves settings: dimensions={0:D}, maxLevels={1:D}, min={2}, max={3}", DimensionsConflict, MaxLevelsConflict, Arrays.ToString( Extents.Min ), Arrays.ToString( Extents.Max ) );
		 }

		 internal class SettingsFromConfig : SpaceFillingCurveSettings
		 {
			  internal SettingsFromConfig( int dimensions, int maxBits, Envelope extents ) : base( dimensions, extents, CalcMaxLevels( dimensions, maxBits ) )
			  {
			  }

			  internal static int CalcMaxLevels( int dimensions, int maxBits )
			  {
					int maxConfigured = maxBits / dimensions;
					int maxSupported = ( dimensions == 2 ) ? HilbertSpaceFillingCurve2D.MAX_LEVEL : HilbertSpaceFillingCurve3D.MAX_LEVEL;
					return Math.Min( maxConfigured, maxSupported );
			  }
		 }

		 internal class SettingsFromIndexHeader : SpaceFillingCurveSettings
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string FailureMessageConflict;

			  internal SettingsFromIndexHeader() : base(0, null, 0)
			  {
			  }

			  internal virtual void MarkAsFailed( string failureMessage )
			  {
					this.FailureMessageConflict = failureMessage;
			  }

			  internal virtual void MarkAsSucceeded()
			  {
					this.FailureMessageConflict = null;
			  }

			  /// <summary>
			  /// The settings are read from the GBPTree header structure, but when this is a FAILED index, there are no settings, but instead an error message
			  /// describing the failure. If that happens, code that triggered the read should check this field and react accordingly. If the the value is null, there
			  /// was no failure.
			  /// </summary>
			  internal virtual string FailureMessage
			  {
				  get
				  {
						return FailureMessageConflict;
				  }
			  }

			  /// <summary>
			  /// The settings are read from the GBPTree header structure, but when this is a FAILED index, there are no settings, but instead an error message
			  /// describing the failure. If that happens, code that triggered the read should check this. If the value is true, calling getFailureMessage() will
			  /// provide an error message describing the failure.
			  /// </summary>
			  internal virtual bool Failed
			  {
				  get
				  {
						return !string.ReferenceEquals( FailureMessageConflict, null );
				  }
			  }

			  internal virtual Header.Reader HeaderReader( System.Func<ByteBuffer, string> onError )
			  {
					return headerBytes =>
					{
					 sbyte state = headerBytes.get();
					 if ( state == BYTE_FAILED )
					 {
						  this.FailureMessageConflict = "Unexpectedly trying to read the header of a failed index: " + onError( headerBytes );
					 }
					 else
					 {
						  int typeId = headerBytes.Int;
						  SpatialIndexType indexType = SpatialIndexType.get( typeId );
						  if ( indexType == null )
						  {
								MarkAsFailed( "Unknown spatial index type in index header: " + typeId );
						  }
						  else
						  {
								MarkAsSucceeded();
								indexType.readHeader( this, headerBytes );
						  }
					 }
					};
			  }
		 }
	}

}