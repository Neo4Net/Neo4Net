using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.proc.temporal
{

	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Org.Neo4j.@internal.Kernel.Api.procs.FieldSignature;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using UserFunctionSignature = Org.Neo4j.@internal.Kernel.Api.procs.UserFunctionSignature;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using CallableUserFunction = Org.Neo4j.Kernel.api.proc.CallableUserFunction;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using IntegralValue = Org.Neo4j.Values.Storable.IntegralValue;
	using Org.Neo4j.Values.Storable;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.FieldSignature.inputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTDateTime;

	[Description("Create a DateTime instant.")]
	internal class DateTimeFunction : TemporalFunction<DateTimeValue>
	{
		 internal DateTimeFunction( System.Func<ZoneId> defaultZone ) : base( NTDateTime, defaultZone )
		 {
		 }

		 protected internal override DateTimeValue Now( Clock clock, string timezone, System.Func<ZoneId> defaultZone )
		 {
			  return string.ReferenceEquals( timezone, null ) ? DateTimeValue.now( clock, defaultZone ) : DateTimeValue.now( clock, timezone );
		 }

		 protected internal override DateTimeValue Parse( TextValue value, System.Func<ZoneId> defaultZone )
		 {
			  return DateTimeValue.parse( value, defaultZone );
		 }

		 protected internal override DateTimeValue Build( MapValue map, System.Func<ZoneId> defaultZone )
		 {
			  return DateTimeValue.build( map, defaultZone );
		 }

		 protected internal override DateTimeValue Select( AnyValue from, System.Func<ZoneId> defaultZone )
		 {
			  return DateTimeValue.select( from, defaultZone );
		 }

		 protected internal override DateTimeValue Truncate( TemporalUnit unit, TemporalValue input, MapValue fields, System.Func<ZoneId> defaultZone )
		 {
			  return DateTimeValue.truncate( unit, input, fields, defaultZone );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void registerMore(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 internal override void RegisterMore( Procedures procedures )
		 {
			  procedures.Register( new FromEpoch() );
			  procedures.Register( new FromEpochMillis() );
		 }

		 private class FromEpoch : CallableUserFunction
		 {
			  internal const string DESCRIPTION = "Create a DateTime given the seconds and nanoseconds since the start of the epoch.";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly IList<FieldSignature> SignatureConflict = Arrays.asList( inputField( "seconds", Neo4jTypes.NTNumber ), inputField( "nanoseconds", Neo4jTypes.NTNumber ) );
			  internal readonly UserFunctionSignature Signature;

			  internal FromEpoch()
			  {
					this.Signature = new UserFunctionSignature( new QualifiedName( new string[] { "datetime" }, "fromepoch" ), SignatureConflict, Neo4jTypes.NTDateTime, null, new string[0], DESCRIPTION, true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return Signature;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  public override AnyValue Apply( Context ctx, AnyValue[] input )
			  {
					if ( input != null && input.Length == 2 )
					{
						 if ( input[0] is IntegralValue && input[1] is IntegralValue )
						 {
							  IntegralValue seconds = ( IntegralValue ) input[0];
							  IntegralValue nanoseconds = ( IntegralValue ) input[1];
							  return DateTimeValue.ofEpoch( seconds, nanoseconds );
						 }
					}
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }

		 private class FromEpochMillis : CallableUserFunction
		 {
			  internal const string DESCRIPTION = "Create a DateTime given the milliseconds since the start of the epoch.";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly IList<FieldSignature> SignatureConflict = Collections.singletonList( inputField( "milliseconds", Neo4jTypes.NTNumber ) );
			  internal readonly UserFunctionSignature Signature;

			  internal FromEpochMillis()
			  {
					this.Signature = new UserFunctionSignature( new QualifiedName( new string[] { "datetime" }, "fromepochmillis" ), SignatureConflict, Neo4jTypes.NTDateTime, null, new string[0], DESCRIPTION, true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return Signature;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.AnyValue apply(org.neo4j.kernel.api.proc.Context ctx, org.neo4j.values.AnyValue[] input) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  public override AnyValue Apply( Context ctx, AnyValue[] input )
			  {
					if ( input != null && input.Length == 1 )
					{
						 if ( input[0] is IntegralValue )
						 {
							  IntegralValue milliseconds = ( IntegralValue ) input[0];
							  return DateTimeValue.ofEpochMillis( milliseconds );
						 }
					}
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }
	}

}