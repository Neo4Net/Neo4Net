using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	/// <summary>
	/// Configuration for <seealso cref="CsvInput"/>.
	/// </summary>
	public interface Configuration : Neo4Net.Csv.Reader.Configuration
	{
		 /// <summary>
		 /// Delimiting character between each values in a CSV input line.
		 /// Typical character is '\t' (TAB) or ',' (it is Comma Separated Values after all).
		 /// </summary>
		 char Delimiter();

		 /// <summary>
		 /// Character separating array values from one another for values that represent arrays.
		 /// </summary>
		 char ArrayDelimiter();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Configuration COMMAS = new Default()
	//	 {
	//		  @@Override public char delimiter()
	//		  {
	//				return ',';
	//		  }
	//
	//		  @@Override public char arrayDelimiter()
	//		  {
	//				return ';';
	//		  }
	//	 };

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Configuration TABS = new Default()
	//	 {
	//		  @@Override public char delimiter()
	//		  {
	//				return '\t';
	//		  }
	//
	//		  @@Override public char arrayDelimiter()
	//		  {
	//				return ',';
	//		  }
	//	 };
	}

	 public abstract class Configuration_Default : Neo4Net.Csv.Reader.Configuration_Default, Configuration
	 {
		 public abstract long CalculateMaxMemoryFromPercent( int percent );
		 public abstract bool CanDetectFreeMemory();
		 public abstract Configuration WithBatchSize( Neo4Net.@unsafe.Impl.Batchimport.Configuration config, int batchSize );
		 public abstract bool AllowCacheAllocationOnHeap();
		 public abstract bool HighIO();
		 public abstract bool ParallelRecordReads();
		 public abstract bool ParallelRecordWrites();
		 public abstract bool SequentialBackgroundFlushing();
		 public abstract long MaxMemoryUsage();
		 public abstract long PageCacheMemory();
		 public abstract int DenseNodeThreshold();
		 public abstract int AllAvailableProcessors();
		 public abstract int MaxNumberOfProcessors();
		 public abstract int MovingAverageSize();
		 public abstract int BatchSize();
	 }

	 public class Configuration_Overridden : Neo4Net.Csv.Reader.Configuration_Overridden, Configuration
	 {
		  internal readonly new Configuration Defaults;

		  private class ValueTypeAnonymousInnerClass : ValueType
		  {
			  private readonly ValueType outerInstance;

			  public ValueTypeAnonymousInnerClass( ValueType outerInstance )
			  {
				  this.outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object read(ReadableClosableChannel from) throws IOException
			  public override object read( ReadableClosableChannel from )
			  {
					ValueType componentType = typeOf( from.get() );
					int length = from.Int;
					object value = Array.CreateInstance( componentType.componentClass(), length );
					for ( int i = 0; i < length; i++ )
					{
						 ( ( Array )value ).SetValue( componentType.Read( from ), i );
					}
					return value;
			  }

			  public override int length( object value )
			  {
					ValueType componentType = typeOf( value.GetType().GetElementType() );
					int arrayLength = Array.getLength( value );
					int length = Byte.BYTES + Integer.BYTES; //array length
					for ( int i = 0; i < arrayLength; i++ )
					{
						 length += componentType.Length( Array.get( value, i ) );
					}
					return length;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(Object value, FlushableChannel into) throws IOException
			  public override void write( object value, FlushableChannel into )
			  {
					ValueType componentType = typeOf( value.GetType().GetElementType() );
					into.put( componentType.Id() );
					int length = Array.getLength( value );
					into.putInt( length );
					for ( int i = 0; i < length; i++ )
					{
						 componentType.Write( Array.get( value, i ), into );
					}
			  }
		  }

		  public Configuration_Overridden( Configuration defaults ) : base( defaults )
		  {
				this.Defaults = defaults;
		  }

		  public override char Delimiter()
		  {
				return Defaults.delimiter();
		  }

		  public override char ArrayDelimiter()
		  {
				return Defaults.arrayDelimiter();
		  }
	 }

}