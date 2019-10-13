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
namespace Neo4Net.Test.rule
{
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using MultipleFailureException = org.junit.runners.model.MultipleFailureException;
	using Statement = org.junit.runners.model.Statement;


	using Exceptions = Neo4Net.Helpers.Exceptions;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueType = Neo4Net.Values.Storable.ValueType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Like a <seealso cref="System.Random"/> but guarantees to include the seed with the test failure, which helps
	/// greatly in debugging.
	/// 
	/// Available methods directly on this class include those found in <seealso cref="RandomValues"/> and the basic ones in <seealso cref="System.Random"/>.
	/// </summary>
	public class RandomRule : TestRule
	{
		 private long _globalSeed;
		 private long _seed;
		 private bool _hasGlobalSeed;
		 private Random _random;
		 private RandomValues _randoms;

		 private RandomValues.Configuration _config = RandomValues.DEFAULT_CONFIGURATION;

		 public virtual RandomRule WithConfiguration( RandomValues.Configuration config )
		 {
			  this._config = config;
			  return this;
		 }

		 public virtual RandomRule WithSeedForAllTests( long seed )
		 {
			  _hasGlobalSeed = true;
			  this._globalSeed = seed;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly RandomRule _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( RandomRule outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  if ( !_outerInstance.hasGlobalSeed )
				  {
						Seed methodSeed = _description.getAnnotation( typeof( Seed ) );
						if ( methodSeed != null )
						{
							 outerInstance.Seed = methodSeed.value();
						}
						else
						{
							 outerInstance.Seed = currentTimeMillis();
						}
				  }
				  else
				  {
						_outerInstance.Seed = _outerInstance.globalSeed;
				  }
				  try
				  {
						@base.evaluate();
				  }
				  catch ( Exception t )
				  {
						if ( t is MultipleFailureException )
						{
							 MultipleFailureException multipleFailures = ( MultipleFailureException ) t;
							 foreach ( Exception failure in multipleFailures.Failures )
							 {
								  enhanceFailureWithSeed( failure );
							 }
						}
						else
						{
							 enhanceFailureWithSeed( t );
						}
						throw t;
				  }
			 }

			 private void enhanceFailureWithSeed( Exception t )
			 {
				  Exceptions.withMessage( t, t.Message + ": random seed used:" + _outerInstance.seed + "L" );
			 }
		 }

		 // ============================
		 // Methods from Random
		 // ============================

		 public virtual void NextBytes( sbyte[] bytes )
		 {
			  _random.NextBytes( bytes );
		 }

		 public virtual bool NextBoolean()
		 {
			  return _random.nextBoolean();
		 }

		 public virtual double NextDouble()
		 {
			  return _random.NextDouble();
		 }

		 public virtual DoubleStream Doubles( int dimension, double minValue, double maxValue )
		 {
			  return _random.doubles( dimension, minValue, maxValue );
		 }

		 public virtual float NextFloat()
		 {
			  return _random.nextFloat();
		 }

		 public virtual int NextInt()
		 {
			  return _random.Next();
		 }

		 public virtual int NextInt( int n )
		 {
			  return _random.Next( n );
		 }

		 public virtual int NextInt( int origin, int bound )
		 {
			  return _random.Next( ( bound - origin ) + 1 ) + origin;
		 }

		 public virtual IntStream Ints( long streamSize, int randomNumberOrigin, int randomNumberBound )
		 {
			  return _random.ints( streamSize, randomNumberOrigin, randomNumberBound );
		 }

		 public virtual double NextGaussian()
		 {
			  return _random.nextGaussian();
		 }

		 public virtual long NextLong()
		 {
			  return _random.nextLong();
		 }

		 public virtual long NextLong( long n )
		 {
			  return Math.Abs( NextLong() ) % n;
		 }

		 public virtual long NextLong( long origin, long bound )
		 {
			  return NextLong( ( bound - origin ) + 1L ) + origin;
		 }

		 // ============================
		 // Methods from RandomValues
		 // ============================

		 public virtual int IntBetween( int min, int max )
		 {
			  return _randoms.intBetween( min, max );
		 }

		 public virtual string NextString()
		 {
			  return NextTextValue().stringValue();
		 }

		 public virtual TextValue NextTextValue()
		 {
			  return _randoms.nextTextValue();
		 }

		 public virtual string NextAlphaNumericString()
		 {
			  return NextAlphaNumericTextValue().stringValue();
		 }

		 public virtual string NextAsciiString()
		 {
			  return NextAsciiTextValue().stringValue();
		 }

		 private TextValue NextAsciiTextValue()
		 {
			  return _randoms.nextAsciiTextValue();
		 }

		 public virtual TextValue NextAlphaNumericTextValue()
		 {
			  return _randoms.nextAlphaNumericTextValue();
		 }

		 public virtual string NextAlphaNumericString( int minLength, int maxLength )
		 {
			  return NextAlphaNumericTextValue( minLength, maxLength ).stringValue();
		 }

		 public virtual TextValue NextAlphaNumericTextValue( int minLength, int maxLength )
		 {
			  return _randoms.nextAlphaNumericTextValue( minLength, maxLength );
		 }

		 public virtual TextValue NextBasicMultilingualPlaneTextValue()
		 {
			  return _randoms.nextBasicMultilingualPlaneTextValue();
		 }

		 public virtual string NextBasicMultilingualPlaneString()
		 {
			  return NextBasicMultilingualPlaneTextValue().stringValue();
		 }

		 public virtual T[] Selection<T>( T[] among, int min, int max, bool allowDuplicates )
		 {
			  return _randoms.selection( among, min, max, allowDuplicates );
		 }

		 public virtual T Among<T>( T[] among )
		 {
			  return _randoms.among( among );
		 }

		 public virtual T Among<T>( IList<T> among )
		 {
			  return _randoms.among( among );
		 }

		 public virtual void Among<T>( IList<T> among, System.Action<T> action )
		 {
			  _randoms.among( among, action );
		 }

		 public virtual object NextValueAsObject()
		 {
			  return _randoms.nextValue().asObject();
		 }

		 public virtual Value NextValue()
		 {
			  return _randoms.nextValue();
		 }

		 public virtual Value NextValue( ValueType type )
		 {
			  return _randoms.nextValueOfType( type );
		 }

		 // ============================
		 // Other utility methods
		 // ============================

		 public virtual void Reset()
		 {
			  _random = new Random( _seed );
			  _randoms = RandomValues.create( _random, _config );
		 }

		 public virtual long Seed()
		 {
			  return _seed;
		 }

		 public virtual Random Random()
		 {
			  return _random;
		 }

		 public virtual RandomValues RandomValues()
		 {
			  return _randoms;
		 }

		 public virtual long Seed
		 {
			 set
			 {
				  this._seed = value;
				  Reset();
			 }
		 }

		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 public class Seed : System.Attribute
		 {
			 private readonly RandomRule _outerInstance;

			 public Seed;
			 {
			 }

			  internal long value;

			 public Seed( public Seed, long value )
			 {
				 this.Seed = Seed;
				 this.value = value;
			 }
		 }
	}

}