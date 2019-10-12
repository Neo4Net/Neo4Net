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
namespace Neo4Net.Values.Storable
{

	using Matcher = org.hamcrest.Matcher;
	using TypeSafeDiagnosingMatcher = org.hamcrest.TypeSafeDiagnosingMatcher;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class FrozenClockRule : Clock, TestRule, System.Func<string, Clock>, System.Func<ZoneId>
	{
		 [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
		 internal class TimeZone : System.Attribute
		 {
			 private readonly FrozenClockRule _outerInstance;

			 public TimeZone;
			 {
			 }

			  internal string[] value;

			 public TimeZone( public TimeZone, String[] value )
			 {
				 this.TimeZone = TimeZone;
				 this.value = value;
			 }
		 }

		 public static readonly TemporalAdjuster SecondPrecision = temporal => temporal.with( ChronoField.NANO_OF_SECOND, 0 );
		 public static readonly TemporalAdjuster MillisecondPrecision = temporal => temporal.with( ChronoField.NANO_OF_SECOND, temporal.get( ChronoField.MILLI_OF_SECOND ) * 1000_000 );
		 private Instant _instant;
		 private ZoneId _zone;

		 public override ZoneId Zone
		 {
			 get
			 {
				  return _zone;
			 }
		 }

		 public virtual Clock WithZone( string zoneId )
		 {
			  return withZone( ZoneId.of( zoneId ) );
		 }

		 public override Clock WithZone( ZoneId zone )
		 {
			  return @fixed( _instant, zone );
		 }

		 public override Instant Instant()
		 {
			  return _instant;
		 }

		 public override long Millis()
		 {
			  return _instant.toEpochMilli();
		 }

		 public virtual Clock At( DateTime datetime )
		 {
			  return at( datetime.atZone( _zone ) );
		 }

		 public virtual Clock At( OffsetDateTime datetime )
		 {
			  return @fixed( datetime.toInstant(), datetime.Offset );
		 }

		 public virtual Clock At( ZonedDateTime datetime )
		 {
			  return @fixed( datetime.toInstant(), datetime.Zone );
		 }

		 public virtual Clock With( TemporalAdjuster adjuster )
		 {
			  return @fixed( _instant.with( adjuster ), _zone );
		 }

		 public virtual Clock With( TemporalField field, long newValue )
		 {
			  return @fixed( _instant.with( field, newValue ), _zone );
		 }

		 public override Clock Apply( string when )
		 {
			  return this;
		 }

		 public override ZoneId Get()
		 {
			  return _zone;
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly FrozenClockRule _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( FrozenClockRule outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  try
				  {
						foreach ( ZoneId zoneId in zonesOf( _description ) )
						{
							 _outerInstance.instant = Instant.now();
							 _outerInstance.zone = zoneId;
							 @base.evaluate();
						}
				  }
				  finally
				  {
						_outerInstance.instant = null;
						_outerInstance.zone = null;
				  }
			 }

			 private ZoneId[] zonesOf( Description description )
			 {
				  TimeZone zone = description.getAnnotation( typeof( TimeZone ) );
				  string[] ids = zone == null ? null : zone.value();
				  if ( ids == null || ids.Length == 0 )
				  {
						return new ZoneId[] { UTC };
				  }
				  else
				  {
						ZoneId[] zones = new ZoneId[ids.Length];
						for ( int i = 0; i < zones.Length; i++ )
						{
							 zones[i] = ZoneId.of( ids[i] );
						}
						return zones;
				  }
			 }
		 }

		 internal static void AssertEqualTemporal<V>( V expected, V actual )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( actual, allOf( equalTo( expected ), EqualOn( "timezone", FrozenClockRule.timezone, expected ), EqualOn( "temporal", TemporalValue::temporal, expected ) ) );
		 }

		 private static ZoneId Timezone<T1>( TemporalValue<T1> temporal )
		 {
			  if ( temporal is DateTimeValue )
			  {
					return ( ( DateTimeValue ) temporal ).Temporal().Zone;
			  }
			  if ( temporal is TimeValue )
			  {
					return ( ( TimeValue ) temporal ).Temporal().Offset;
			  }
			  return null;
		 }

		 private static Matcher<T> EqualOn<T, U>( string trait, System.Func<T, U> mapping, T expected )
		 {
			  return new TypeSafeDiagnosingMatcherAnonymousInnerClass( expected.GetType(), trait, mapping, expected );
		 }

		 private class TypeSafeDiagnosingMatcherAnonymousInnerClass : TypeSafeDiagnosingMatcher<T>
		 {
			 private string _trait;
			 private System.Func<T, U> _mapping;
			 private T _expected;

			 public TypeSafeDiagnosingMatcherAnonymousInnerClass( UnknownType getClass, string trait, System.Func<T, U> mapping, T expected ) : base( getClass )
			 {
				 this._trait = trait;
				 this._mapping = mapping;
				 this._expected = expected;
			 }

			 protected internal override bool matchesSafely( T actual, org.hamcrest.Description mismatchDescription )
			 {
				  U e = _mapping( _expected );
				  U a = _mapping( actual );
				  if ( Objects.Equals( e, a ) )
				  {
						return true;
				  }
				  mismatchDescription.appendText( "- " );
				  mismatchDescription.appendText( "expected: " ).appendValue( e );
				  mismatchDescription.appendText( " but was: " ).appendValue( a );
				  return false;
			 }

			 public override void describeTo( org.hamcrest.Description description )
			 {
				  description.appendText( _trait ).appendText( " should be equal" );
			 }
		 }
	}

}