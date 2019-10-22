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
namespace Neo4Net.Server.helpers
{

	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Point = Neo4Net.GraphDb.spatial.Point;
	using Neo4Net.Helpers.Collections;

	/*
	 * THIS CLASS SHOULD BE MOVED TO KERNEL ASAP!!!
	 */
	public abstract class PropertyTypeDispatcher<K, T>
	{
		 public abstract class PropertyArray<A, T> : IEnumerable<T>
		 {
			  internal PropertyArray()
			  {
			  }

			  public abstract int Length();

			  public abstract A ClonedArray { get; }

			  public abstract Type Type { get; }
		 }

		 public static void ConsumeProperties( PropertyTypeDispatcher<string, Void> dispatcher, IPropertyContainer IEntity )
		 {
			  foreach ( KeyValuePair<string, object> property in IEntity.AllProperties.SetOfKeyValuePairs() )
			  {
					dispatcher.Dispatch( property.Value, property.Key );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public final T dispatch(Object property, K param)
		 public T Dispatch( object property, K param )
		 {
			  if ( property == null )
			  {
					return DispatchNullProperty( param );
			  }
			  else if ( property is string )
			  {
					return DispatchStringProperty( ( string ) property, param );
			  }
			  else if ( property is Number )
			  {
					return DispatchNumberProperty( ( Number ) property, param );
			  }
			  else if ( property is bool? )
			  {
					return DispatchBooleanProperty( ( bool? ) property.Value, param );
			  }
			  else if ( property is char? )
			  {
					return DispatchCharacterProperty( ( char? ) property.Value, param );
			  }
			  else if ( property is Point )
			  {
				 return DispatchPointProperty( ( Point ) property, param );
			  }
			  else if ( property is Temporal )
			  {
					return DispatchTemporalProperty( ( Temporal ) property, param );
			  }
			  else if ( property is TemporalAmount )
			  {
					return DispatchTemporalAmountProperty( ( TemporalAmount ) property, param );
			  }
			  else if ( property is string[] )
			  {
					return DispatchStringArrayProperty( ( string[] ) property, param );
			  }
			  else if ( property is Point[] )
			  {
					return DispatchPointArrayProperty( ( Point[] ) property, param );
			  }
			  else if ( property is Temporal[] )
			  {
					return DispatchTemporalArrayProperty( ( Temporal[] ) property, param );

			  }
			  else if ( property is TemporalAmount[] )
			  {
					return DispatchTemporalAmountArrayProperty( ( TemporalAmount[] ) property, param );
			  }
			  else if ( property is object[] )
			  {
					return DispatchOtherArray( ( object[] ) property, param );
			  }
			  else
			  {
					Type propertyType = property.GetType();
					if ( propertyType.IsArray && propertyType.GetElementType().Primitive )
					{
						 return DispatchPrimitiveArray( property, param );
					}
					else
					{
						 return DispatchOtherProperty( property, param );
					}
			  }
		 }

		 private T DispatchPrimitiveArray( object property, K param )
		 {
			  if ( property is sbyte[] )
			  {
					return DispatchByteArrayProperty( ( sbyte[] ) property, param );
			  }
			  else if ( property is char[] )
			  {
					return DispatchCharacterArrayProperty( ( char[] ) property, param );
			  }
			  else if ( property is bool[] )
			  {
					return DispatchBooleanArrayProperty( ( bool[] ) property, param );
			  }
			  else if ( property is long[] )
			  {
					return DispatchLongArrayProperty( ( long[] ) property, param );
			  }
			  else if ( property is double[] )
			  {
					return DispatchDoubleArrayProperty( ( double[] ) property, param );
			  }
			  else if ( property is int[] )
			  {
					return DispatchIntegerArrayProperty( ( int[] ) property, param );
			  }
			  else if ( property is short[] )
			  {
					return DispatchShortArrayProperty( ( short[] ) property, param );
			  }
			  else if ( property is float[] )
			  {
					return DispatchFloatArrayProperty( ( float[] ) property, param );
			  }
			  else
			  {
					throw new Exception( "Unsupported primitive array type: " + property.GetType() );
			  }
		 }

		 protected internal virtual T DispatchOtherArray( object[] property, K param )
		 {
			  if ( property is sbyte?[] )
			  {
					return DispatchByteArrayProperty( ( sbyte?[] ) property, param );
			  }
			  else if ( property is char?[] )
			  {
					return DispatchCharacterArrayProperty( ( char?[] ) property, param );
			  }
			  else if ( property is bool?[] )
			  {
					return DispatchBooleanArrayProperty( ( bool?[] ) property, param );
			  }
			  else if ( property is long?[] )
			  {
					return DispatchLongArrayProperty( ( long?[] ) property, param );
			  }
			  else if ( property is double?[] )
			  {
					return DispatchDoubleArrayProperty( ( double?[] ) property, param );
			  }
			  else if ( property is int?[] )
			  {
					return DispatchIntegerArrayProperty( ( int?[] ) property, param );
			  }
			  else if ( property is short?[] )
			  {
					return DispatchShortArrayProperty( ( short?[] ) property, param );
			  }
			  else if ( property is float?[] )
			  {
					return DispatchFloatArrayProperty( ( float?[] ) property, param );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unsupported property array type: " + property.GetType() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected T dispatchNumberProperty(Number property, K param)
		 protected internal virtual T DispatchNumberProperty( Number property, K param )
		 {
			  if ( property is long? )
			  {
					return DispatchLongProperty( ( long? ) property.Value, param );
			  }
			  else if ( property is int? )
			  {
					return DispatchIntegerProperty( ( int? ) property.Value, param );
			  }
			  else if ( property is double? )
			  {
					return DispatchDoubleProperty( ( double? ) property.Value, param );
			  }
			  else if ( property is float? )
			  {
					return DispatchFloatProperty( ( float? ) property.Value, param );
			  }
			  else if ( property is short? )
			  {
					return DispatchShortProperty( ( short? ) property.Value, param );
			  }
			  else if ( property is sbyte? )
			  {
					return DispatchByteProperty( ( sbyte? ) property.Value, param );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unsupported property type: " + property.GetType() );
			  }
		 }

		 protected internal virtual T DispatchNullProperty( K param )
		 {
			  return default( T );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchByteProperty(byte property, K param);
		 protected internal abstract T DispatchByteProperty( sbyte property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchCharacterProperty(char property, K param);
		 protected internal abstract T DispatchCharacterProperty( char property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchShortProperty(short property, K param);
		 protected internal abstract T DispatchShortProperty( short property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchIntegerProperty(int property, K param);
		 protected internal abstract T DispatchIntegerProperty( int property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchLongProperty(long property, K param);
		 protected internal abstract T DispatchLongProperty( long property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchFloatProperty(float property, K param);
		 protected internal abstract T DispatchFloatProperty( float property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchDoubleProperty(double property, K param);
		 protected internal abstract T DispatchDoubleProperty( double property, K param );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") protected abstract T dispatchBooleanProperty(boolean property, K param);
		 protected internal abstract T DispatchBooleanProperty( bool property, K param );

		 //not abstract in order to not break existing code, since this was fixed in point release
		 protected internal virtual T DispatchPointProperty( Point property, K param )
		 {
			  return DispatchOtherProperty( property, param );
		 }

		 //not abstract in order to not break existing code, since this was fixed in point release
		 protected internal virtual T DispatchTemporalProperty( Temporal property, K param )
		 {
			  return DispatchOtherProperty( property, param );
		 }

		 //not abstract in order to not break existing code, since this was fixed in point release
		 protected internal virtual T DispatchTemporalAmountProperty( TemporalAmount property, K param )
		 {
			  return DispatchOtherProperty( property, param );
		 }

		 protected internal virtual T DispatchOtherProperty( object property, K param )
		 {
			  throw new System.ArgumentException( "Unsupported property type: " + property.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchByteArrayProperty(final byte[] property, K param)
		 protected internal virtual T DispatchByteArrayProperty( sbyte[] property, K param )
		 {
			  return dispatchByteArrayProperty(new PrimitiveArrayAnonymousInnerClass(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass : PrimitiveArray<sbyte[], sbyte>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private sbyte[] _property;

			 public PrimitiveArrayAnonymousInnerClass( PropertyTypeDispatcher<K, T> outerInstance, sbyte[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override sbyte[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<sbyte> item(int offset)
			 protected internal override sbyte? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchCharacterArrayProperty(final char[] property, K param)
		 protected internal virtual T DispatchCharacterArrayProperty( char[] property, K param )
		 {
			  return dispatchCharacterArrayProperty(new PrimitiveArrayAnonymousInnerClass2(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass2 : PrimitiveArray<char[], char>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private char[] _property;

			 public PrimitiveArrayAnonymousInnerClass2( PropertyTypeDispatcher<K, T> outerInstance, char[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override char[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<char> item(int offset)
			 protected internal override char? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchShortArrayProperty(final short[] property, K param)
		 protected internal virtual T DispatchShortArrayProperty( short[] property, K param )
		 {
			  return dispatchShortArrayProperty(new PrimitiveArrayAnonymousInnerClass3(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass3 : PrimitiveArray<short[], short>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private short[] _property;

			 public PrimitiveArrayAnonymousInnerClass3( PropertyTypeDispatcher<K, T> outerInstance, short[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override short[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<short> item(int offset)
			 protected internal override short? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchIntegerArrayProperty(final int[] property, K param)
		 protected internal virtual T DispatchIntegerArrayProperty( int[] property, K param )
		 {
			  return dispatchIntegerArrayProperty(new PrimitiveArrayAnonymousInnerClass4(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass4 : PrimitiveArray<int[], int>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private int[] _property;

			 public PrimitiveArrayAnonymousInnerClass4( PropertyTypeDispatcher<K, T> outerInstance, int[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override int[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<int> item(int offset)
			 protected internal override int? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchLongArrayProperty(final long[] property, K param)
		 protected internal virtual T DispatchLongArrayProperty( long[] property, K param )
		 {
			  return dispatchLongArrayProperty(new PrimitiveArrayAnonymousInnerClass5(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass5 : PrimitiveArray<long[], long>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private long[] _property;

			 public PrimitiveArrayAnonymousInnerClass5( PropertyTypeDispatcher<K, T> outerInstance, long[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override long[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<long> item(int offset)
			 protected internal override long? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchFloatArrayProperty(final float[] property, K param)
		 protected internal virtual T DispatchFloatArrayProperty( float[] property, K param )
		 {
			  return dispatchFloatArrayProperty(new PrimitiveArrayAnonymousInnerClass6(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass6 : PrimitiveArray<float[], float>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private float[] _property;

			 public PrimitiveArrayAnonymousInnerClass6( PropertyTypeDispatcher<K, T> outerInstance, float[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override float[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<float> item(int offset)
			 protected internal override float? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchDoubleArrayProperty(final double[] property, K param)
		 protected internal virtual T DispatchDoubleArrayProperty( double[] property, K param )
		 {
			  return dispatchDoubleArrayProperty(new PrimitiveArrayAnonymousInnerClass7(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass7 : PrimitiveArray<double[], double>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private double[] _property;

			 public PrimitiveArrayAnonymousInnerClass7( PropertyTypeDispatcher<K, T> outerInstance, double[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override double[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<double> item(int offset)
			 protected internal override double? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchBooleanArrayProperty(final boolean[] property, K param)
		 protected internal virtual T DispatchBooleanArrayProperty( bool[] property, K param )
		 {
			  return dispatchBooleanArrayProperty(new PrimitiveArrayAnonymousInnerClass8(this, property)
			 , param);
		 }

		 private class PrimitiveArrayAnonymousInnerClass8 : PrimitiveArray<bool[], bool>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private bool[] _property;

			 public PrimitiveArrayAnonymousInnerClass8( PropertyTypeDispatcher<K, T> outerInstance, bool[] property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override bool[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }

			 public override int length()
			 {
				  return _property.Length;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected System.Nullable<bool> item(int offset)
			 protected internal override bool? item( int offset )
			 {
				  return _property[offset];
			 }

			 public override Type Type
			 {
				 get
				 {
					  return _property.GetType();
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchByteArrayProperty(final System.Nullable<sbyte>[] property, K param)
		 protected internal virtual T DispatchByteArrayProperty( sbyte?[] property, K param )
		 {
			  return dispatchByteArrayProperty(new BoxedArrayAnonymousInnerClass(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass : BoxedArray<sbyte[], sbyte>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private sbyte?[] _property;

			 public BoxedArrayAnonymousInnerClass( PropertyTypeDispatcher<K, T> outerInstance, sbyte?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public byte[] getClonedArray()
			 public override sbyte[] ClonedArray
			 {
				 get
				 {
					  sbyte[] result = new sbyte[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchCharacterArrayProperty(final System.Nullable<char>[] property, K param)
		 protected internal virtual T DispatchCharacterArrayProperty( char?[] property, K param )
		 {
			  return dispatchCharacterArrayProperty(new BoxedArrayAnonymousInnerClass2(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass2 : BoxedArray<char[], char>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private char?[] _property;

			 public BoxedArrayAnonymousInnerClass2( PropertyTypeDispatcher<K, T> outerInstance, char?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public char[] getClonedArray()
			 public override char[] ClonedArray
			 {
				 get
				 {
					  char[] result = new char[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchShortArrayProperty(final System.Nullable<short>[] property, K param)
		 protected internal virtual T DispatchShortArrayProperty( short?[] property, K param )
		 {
			  return dispatchShortArrayProperty(new BoxedArrayAnonymousInnerClass3(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass3 : BoxedArray<short[], short>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private short?[] _property;

			 public BoxedArrayAnonymousInnerClass3( PropertyTypeDispatcher<K, T> outerInstance, short?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public short[] getClonedArray()
			 public override short[] ClonedArray
			 {
				 get
				 {
					  short[] result = new short[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchIntegerArrayProperty(final System.Nullable<int>[] property, K param)
		 protected internal virtual T DispatchIntegerArrayProperty( int?[] property, K param )
		 {
			  return dispatchIntegerArrayProperty(new BoxedArrayAnonymousInnerClass4(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass4 : BoxedArray<int[], int>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private int?[] _property;

			 public BoxedArrayAnonymousInnerClass4( PropertyTypeDispatcher<K, T> outerInstance, int?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public int[] getClonedArray()
			 public override int[] ClonedArray
			 {
				 get
				 {
					  int[] result = new int[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchLongArrayProperty(final System.Nullable<long>[] property, K param)
		 protected internal virtual T DispatchLongArrayProperty( long?[] property, K param )
		 {
			  return dispatchLongArrayProperty(new BoxedArrayAnonymousInnerClass5(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass5 : BoxedArray<long[], long>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private long?[] _property;

			 public BoxedArrayAnonymousInnerClass5( PropertyTypeDispatcher<K, T> outerInstance, long?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public long[] getClonedArray()
			 public override long[] ClonedArray
			 {
				 get
				 {
					  long[] result = new long[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchFloatArrayProperty(final System.Nullable<float>[] property, K param)
		 protected internal virtual T DispatchFloatArrayProperty( float?[] property, K param )
		 {
			  return dispatchFloatArrayProperty(new BoxedArrayAnonymousInnerClass6(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass6 : BoxedArray<float[], float>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private float?[] _property;

			 public BoxedArrayAnonymousInnerClass6( PropertyTypeDispatcher<K, T> outerInstance, float?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public float[] getClonedArray()
			 public override float[] ClonedArray
			 {
				 get
				 {
					  float[] result = new float[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchDoubleArrayProperty(final System.Nullable<double>[] property, K param)
		 protected internal virtual T DispatchDoubleArrayProperty( double?[] property, K param )
		 {
			  return dispatchDoubleArrayProperty(new BoxedArrayAnonymousInnerClass7(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass7 : BoxedArray<double[], double>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private double?[] _property;

			 public BoxedArrayAnonymousInnerClass7( PropertyTypeDispatcher<K, T> outerInstance, double?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public double[] getClonedArray()
			 public override double[] ClonedArray
			 {
				 get
				 {
					  double[] result = new double[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchBooleanArrayProperty(final System.Nullable<bool>[] property, K param)
		 protected internal virtual T DispatchBooleanArrayProperty( bool?[] property, K param )
		 {
			  return dispatchBooleanArrayProperty(new BoxedArrayAnonymousInnerClass8(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass8 : BoxedArray<bool[], bool>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private bool?[] _property;

			 public BoxedArrayAnonymousInnerClass8( PropertyTypeDispatcher<K, T> outerInstance, bool?[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public boolean[] getClonedArray()
			 public override bool[] ClonedArray
			 {
				 get
				 {
					  bool[] result = new bool[_property.Length];
					  for ( int i = 0; i < result.Length; i++ )
					  {
							result[i] = _property[i].Value;
					  }
					  return result;
				 }
			 }
		 }

		 protected internal abstract T DispatchStringProperty( string property, K param );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchStringArrayProperty(final String[] property, K param)
		 protected internal virtual T DispatchStringArrayProperty( string[] property, K param )
		 {
			  return dispatchStringArrayProperty(new BoxedArrayAnonymousInnerClass9(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass9 : BoxedArray<string[], string>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private string[] _property;

			 public BoxedArrayAnonymousInnerClass9( PropertyTypeDispatcher<K, T> outerInstance, string[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override string[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }
		 }

		 protected internal virtual T DispatchStringArrayProperty( PropertyArray<string[], string> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchPointArrayProperty(final org.Neo4Net.graphdb.spatial.Point[] property, K param)
		 protected internal virtual T DispatchPointArrayProperty( Point[] property, K param )
		 {
			  return dispatchPointArrayProperty(new BoxedArrayAnonymousInnerClass10(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass10 : BoxedArray<Point[], Point>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private Point[] _property;

			 public BoxedArrayAnonymousInnerClass10( PropertyTypeDispatcher<K, T> outerInstance, Point[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override Point[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }
		 }

		 protected internal virtual T DispatchPointArrayProperty( PropertyArray<Point[], Point> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

		 protected internal virtual T DispatchTemporalArrayProperty( PropertyArray<Temporal[], Temporal> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchTemporalArrayProperty(final java.time.temporal.Temporal[] property, K param)
		 protected internal virtual T DispatchTemporalArrayProperty( Temporal[] property, K param )
		 {
			  return dispatchTemporalArrayProperty(new BoxedArrayAnonymousInnerClass11(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass11 : BoxedArray<Temporal[], Temporal>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private Temporal[] _property;

			 public BoxedArrayAnonymousInnerClass11( PropertyTypeDispatcher<K, T> outerInstance, Temporal[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override Temporal[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }
		 }

		 protected internal virtual T DispatchTemporalAmountArrayProperty( PropertyArray<TemporalAmount[], TemporalAmount> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected T dispatchTemporalAmountArrayProperty(final java.time.temporal.TemporalAmount[] property, K param)
		 protected internal virtual T DispatchTemporalAmountArrayProperty( TemporalAmount[] property, K param )
		 {
			  return dispatchTemporalAmountArrayProperty(new BoxedArrayAnonymousInnerClass12(this, property)
			 , param);
		 }

		 private class BoxedArrayAnonymousInnerClass12 : BoxedArray<TemporalAmount[], TemporalAmount>
		 {
			 private readonly PropertyTypeDispatcher<K, T> _outerInstance;

			 private TemporalAmount[] _property;

			 public BoxedArrayAnonymousInnerClass12( PropertyTypeDispatcher<K, T> outerInstance, TemporalAmount[] property ) : base( property )
			 {
				 this.outerInstance = outerInstance;
				 this._property = property;
			 }

			 public override TemporalAmount[] ClonedArray
			 {
				 get
				 {
					  return _property.Clone();
				 }
			 }
		 }

		 protected internal virtual T DispatchByteArrayProperty( PropertyArray<sbyte[], sbyte> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchCharacterArrayProperty( PropertyArray<char[], char> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

		 protected internal virtual T DispatchShortArrayProperty( PropertyArray<short[], short> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchIntegerArrayProperty( PropertyArray<int[], int> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchLongArrayProperty( PropertyArray<long[], long> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchFloatArrayProperty( PropertyArray<float[], float> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchDoubleArrayProperty( PropertyArray<double[], double> array, K param )
		 {
			  return DispatchNumberArray( array, param );
		 }

		 protected internal virtual T DispatchBooleanArrayProperty( PropertyArray<bool[], bool> array, K param )
		 {
			  return DispatchArray( array, param );
		 }

		 protected internal virtual T DispatchNumberArray<T1>( PropertyArray<T1> array, K param ) where T1 : Number
		 {
			  return DispatchArray( array, param );
		 }

		 protected internal virtual T DispatchArray<T1>( PropertyArray<T1> array, K param )
		 {
			  throw new System.NotSupportedException( "Unhandled array type: " + array.Type );
		 }

		 private abstract class BoxedArray<A, T> : PropertyArray<A, T>
		 {
			  internal readonly T[] Array;

			  internal BoxedArray( T[] array )
			  {
					this.Array = array;
			  }

			  public override int Length()
			  {
					return Array.Length;
			  }

			  public override IEnumerator<T> Iterator()
			  {
					return new ArrayIterator<T>( Array );
			  }

			  public override Type Type
			  {
				  get
				  {
						return Array.GetType();
				  }
			  }
		 }

		 private abstract class PrimitiveArray<A, T> : PropertyArray<A, T>
		 {
			  public override IEnumerator<T> Iterator()
			  {
					return new IteratorAnonymousInnerClass( this );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<T>
			  {
				  private readonly PrimitiveArray<A, T> _outerInstance;

				  public IteratorAnonymousInnerClass( PrimitiveArray<A, T> outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  size = outerInstance.Length();
				  }

				  internal readonly int size;
				  internal int pos;

				  public bool hasNext()
				  {
						return pos < size;
				  }

				  public T next()
				  {
						return outerInstance.Item( pos++ );
				  }

				  public void remove()
				  {
						throw new System.NotSupportedException( "Cannot remove element from primitive array." );
				  }
			  }

			  protected internal abstract T Item( int offset );
		 }
	}

}