using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.proc.temporal
{

	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using DefaultParameterValue = Neo4Net.@internal.Kernel.Api.procs.DefaultParameterValue;
	using FieldSignature = Neo4Net.@internal.Kernel.Api.procs.FieldSignature;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using UserFunctionSignature = Neo4Net.@internal.Kernel.Api.procs.UserFunctionSignature;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Neo4Net.Kernel.api.proc;
	using Description = Neo4Net.Procedure.Description;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.DefaultParameterValue.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.FieldSignature.inputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public abstract class TemporalFunction<T> : CallableUserFunction where T : Neo4Net.Values.AnyValue
	{
		 private const string DEFAULT_TEMPORAL_ARGUMENT = "DEFAULT_TEMPORAL_ARGUMENT";
		 private static readonly TextValue _defaultTemporalArgumentValue = Values.stringValue( DEFAULT_TEMPORAL_ARGUMENT );
		 private static readonly DefaultParameterValue _defaultParameterValue = new DefaultParameterValue( DEFAULT_TEMPORAL_ARGUMENT, Neo4jTypes.NTAny );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void registerTemporalFunctions(org.neo4j.kernel.impl.proc.Procedures procedures, org.neo4j.kernel.impl.proc.ProcedureConfig procedureConfig) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public static void RegisterTemporalFunctions( Procedures procedures, ProcedureConfig procedureConfig )
		 {
			  System.Func<ZoneId> defaultZone = procedureConfig.getDefaultTemporalTimeZone;
			  Register( new DateTimeFunction( defaultZone ), procedures );
			  Register( new LocalDateTimeFunction( defaultZone ), procedures );
			  Register( new DateFunction( defaultZone ), procedures );
			  Register( new TimeFunction( defaultZone ), procedures );
			  Register( new LocalTimeFunction( defaultZone ), procedures );
			  DurationFunction.Register( procedures );
		 }

		 private static readonly Key<Clock> _defaultClock = Neo4Net.Kernel.api.proc.Context_Fields.StatementClock;

		 /// <param name="clock"> the clock to use </param>
		 /// <param name="timezone"> an explicit timezone or {@code null}. In the latter case, the defaultZone is used </param>
		 /// <param name="defaultZone"> configured default time zone. </param>
		 /// <returns> the current time/date </returns>
		 protected internal abstract T Now( Clock clock, string timezone, System.Func<ZoneId> defaultZone );

		 protected internal abstract T Parse( TextValue value, System.Func<ZoneId> defaultZone );

		 protected internal abstract T Build( MapValue map, System.Func<ZoneId> defaultZone );

		 protected internal abstract T Select( AnyValue from, System.Func<ZoneId> defaultZone );

		 protected internal abstract T Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone );

		 private static readonly IList<FieldSignature> _inputSignature = singletonList( inputField( "input", Neo4jTypes.NTAny, _defaultParameterValue ) );
		 private static readonly string[] _allowed = new string[] {};

		 private readonly UserFunctionSignature _signature;
		 private readonly System.Func<ZoneId> _defaultZone;

		 internal TemporalFunction( Neo4jTypes.AnyType result, System.Func<ZoneId> defaultZone )
		 {
			  string basename = basename( this.GetType() );
			  Debug.Assert( result.GetType().Name.Equals(basename + "Type"), "result type should match function name" );
			  Description description = this.GetType().getAnnotation(typeof(Description));
			  this._signature = new UserFunctionSignature( new QualifiedName( new string[0], basename.ToLower() ), _inputSignature, result, null, _allowed, description == null ? null : description.value(), true );
			  this._defaultZone = defaultZone;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void register(TemporalFunction<?> super, org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private static void Register<T1>( TemporalFunction<T1> @base, Procedures procedures )
		 {
			  procedures.Register( @base );
			  procedures.Register( new Now<>( @base, "transaction" ) );
			  procedures.Register( new Now<>( @base, "statement" ) );
			  procedures.Register( new Now<>( @base, "realtime" ) );
			  procedures.Register( new Truncate<>( @base ) );
			  @base.RegisterMore( procedures );
		 }

		 private static string Basename( Type function )
		 {
			  return function.Name.Replace( "Function", "" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void registerMore(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 internal virtual void RegisterMore( Procedures procedures )
		 {
			  // Empty by default
		 }

		 public override UserFunctionSignature Signature()
		 {
			  return _signature;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override AnyValue Apply( Context ctx, AnyValue[] input )
		 {
			  if ( input == null || ( input.Length > 0 && ( input[0] == NO_VALUE || input[0] == null ) ) )
			  {
					return NO_VALUE;
			  }
			  else if ( input.Length == 0 || input[0].Equals( _defaultTemporalArgumentValue ) )
			  {
					return Now( ctx.Get( _defaultClock ), null, _defaultZone );
			  }
			  else if ( input[0] is TextValue )
			  {
					return Parse( ( TextValue ) input[0], _defaultZone );
			  }
			  else if ( input[0] is TemporalValue )
			  {
					return Select( input[0], _defaultZone );
			  }
			  else if ( input[0] is MapValue )
			  {
					MapValue map = ( MapValue ) input[0];
					string timezone = OnlyTimezone( map );
					if ( !string.ReferenceEquals( timezone, null ) )
					{
						 return Now( ctx.Get( _defaultClock ), timezone, _defaultZone );
					}
					return Build( map, _defaultZone );
			  }
			  else
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }

		 private static string OnlyTimezone( MapValue map )
		 {
			  if ( map.Size() == 1 )
			  {
					string key = single( map.Keys );
					if ( "timezone".Equals( key, StringComparison.OrdinalIgnoreCase ) )
					{
						 AnyValue timezone = map.Get( key );
						 if ( timezone is TextValue )
						 {
							  return ( ( TextValue ) timezone ).stringValue();
						 }
					}
			  }
			  return null;
		 }

		 private abstract class SubFunction<T> : CallableUserFunction where T : Neo4Net.Values.AnyValue
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly UserFunctionSignature SignatureConflict;
			  internal readonly TemporalFunction<T> Function;

			  internal SubFunction( TemporalFunction<T> @base, string name, IList<FieldSignature> input, string description )
			  {
					this.Function = @base;
					this.SignatureConflict = new UserFunctionSignature( new QualifiedName( new string[] { @base.signature.Name().name() }, name ), input, @base.signature.OutputType(), null, _allowed, description, true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return SignatureConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException;
			  public override abstract AnyValue Apply( Context ctx, AnyValue[] input );
		 }

		 private class Now<T> : SubFunction<T> where T : Neo4Net.Values.AnyValue
		 {
			  internal static readonly IList<FieldSignature> Signature = singletonList( inputField( "timezone", Neo4jTypes.NTAny, _defaultParameterValue ) );
			  internal readonly Key<Clock> Key;

			  internal Now( TemporalFunction<T> function, string clock ) : base( function, clock, Signature, string.Format( "Get the current {0} instant using the {1} clock.", Basename( function.GetType() ), clock ) )
			  {
					switch ( clock )
					{
					case "transaction":
						 this.Key = Neo4Net.Kernel.api.proc.Context_Fields.TransactionClock;
						 break;
					case "statement":
						 this.Key = Neo4Net.Kernel.api.proc.Context_Fields.StatementClock;
						 break;
					case "realtime":
						 this.Key = Neo4Net.Kernel.api.proc.Context_Fields.SystemClock;
						 break;
					default:
						 throw new System.ArgumentException( "Unrecognized clock: " + clock );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  public override AnyValue Apply( Context ctx, AnyValue[] input )
			  {
					if ( input == null || ( input.Length > 0 && ( input[0] == NO_VALUE || input[0] == null ) ) )
					{
						 return NO_VALUE;
					}
					else if ( input.Length == 0 || input[0].Equals( _defaultTemporalArgumentValue ) )
					{
						 return function.now( ctx.Get( Key ), null, function.defaultZone );
					}
					else if ( input.Length == 1 && input[0] is TextValue )
					{
						 TextValue timezone = ( TextValue ) input[0];
						 return function.now( ctx.Get( Key ), timezone.StringValue(), function.defaultZone );
					}
					else
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
					}
			  }
		 }

		 private class Truncate<T> : SubFunction<T> where T : Neo4Net.Values.AnyValue
		 {
			  internal static readonly IList<FieldSignature> Signature = Arrays.asList( inputField( "unit", Neo4jTypes.NTString ), inputField( "input", Neo4jTypes.NTAny ), inputField( "fields", Neo4jTypes.NTMap, nullValue( Neo4jTypes.NTMap ) ) );

			  internal Truncate( TemporalFunction<T> function ) : base( function, "truncate", Signature, string.Format( "Truncate the input temporal value to a {0} instant using the specified unit.", Basename( function.GetType() ) ) )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] args) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  public override T Apply( Context ctx, AnyValue[] args )
			  {
					if ( args != null && args.Length >= 2 && args.Length <= 3 )
					{
						 AnyValue unit = args[0];
						 AnyValue input = args[1];
						 AnyValue fields = args.Length == 2 || args[2] == NO_VALUE ? EMPTY_MAP : args[2];
						 if ( unit is TextValue && input is TemporalValue && fields is MapValue )
						 {
							  return function.truncate( unit( ( ( TextValue ) unit ).stringValue() ), (TemporalValue)input, (MapValue) fields, function.defaultZone );
						 }
					}
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(args) );
			  }

			  internal static TemporalUnit Unit( string unit )
			  {
					switch ( unit )
					{
					case "millennium":
						 return ChronoUnit.MILLENNIA;
					case "century":
						 return ChronoUnit.CENTURIES;
					case "decade":
						 return ChronoUnit.DECADES;
					case "year":
						 return ChronoUnit.YEARS;
					case "weekYear":
						 return IsoFields.WEEK_BASED_YEARS;
					case "quarter":
						 return IsoFields.QUARTER_YEARS;
					case "month":
						 return ChronoUnit.MONTHS;
					case "week":
						 return ChronoUnit.WEEKS;
					case "day":
						 return ChronoUnit.DAYS;
					case "hour":
						 return ChronoUnit.HOURS;
					case "minute":
						 return ChronoUnit.MINUTES;
					case "second":
						 return ChronoUnit.SECONDS;
					case "millisecond":
						 return ChronoUnit.MILLIS;
					case "microsecond":
						 return ChronoUnit.MICROS;
					default:
						 throw new System.ArgumentException( "Unsupported unit: " + unit );
					}
			  }
		 }
	}

}