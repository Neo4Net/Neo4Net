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
namespace Neo4Net.Kernel.impl.proc.temporal
{

	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using FieldSignature = Neo4Net.Kernel.Api.Internal.procs.FieldSignature;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using QualifiedName = Neo4Net.Kernel.Api.Internal.procs.QualifiedName;
	using UserFunctionSignature = Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableUserFunction = Neo4Net.Kernel.api.proc.CallableUserFunction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Description = Neo4Net.Procedure.Description;
	using AnyValue = Neo4Net.Values.AnyValue;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using Neo4Net.Values.Storable;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.FieldSignature.inputField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDateTime;

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
//ORIGINAL LINE: void registerMore(org.Neo4Net.kernel.impl.proc.Procedures procedures) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 internal override void RegisterMore( Procedures procedures )
		 {
			  procedures.Register( new FromEpoch() );
			  procedures.Register( new FromEpochMillis() );
		 }

		 private class FromEpoch : CallableUserFunction
		 {
			  internal const string DESCRIPTION = "Create a DateTime given the seconds and nanoseconds since the start of the epoch.";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly IList<FieldSignature> SignatureConflict = Arrays.asList( inputField( "seconds", Neo4NetTypes.NTNumber ), inputField( "nanoseconds", Neo4NetTypes.NTNumber ) );
			  internal readonly UserFunctionSignature Signature;

			  internal FromEpoch()
			  {
					this.Signature = new UserFunctionSignature( new QualifiedName( new string[] { "datetime" }, "fromepoch" ), SignatureConflict, Neo4NetTypes.NTDateTime, null, new string[0], DESCRIPTION, true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return Signature;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue apply(org.Neo4Net.kernel.api.proc.Context ctx, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
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
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }

		 private class FromEpochMillis : CallableUserFunction
		 {
			  internal const string DESCRIPTION = "Create a DateTime given the milliseconds since the start of the epoch.";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal static readonly IList<FieldSignature> SignatureConflict = Collections.singletonList( inputField( "milliseconds", Neo4NetTypes.NTNumber ) );
			  internal readonly UserFunctionSignature Signature;

			  internal FromEpochMillis()
			  {
					this.Signature = new UserFunctionSignature( new QualifiedName( new string[] { "datetime" }, "fromepochmillis" ), SignatureConflict, Neo4NetTypes.NTDateTime, null, new string[0], DESCRIPTION, true );
			  }

			  public override UserFunctionSignature Signature()
			  {
					return Signature;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.values.AnyValue apply(org.Neo4Net.kernel.api.proc.Context ctx, org.Neo4Net.values.AnyValue[] input) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
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
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Invalid call signature for " + this.GetType().Name + ": Provided input was " + Arrays.ToString(input) );
			  }
		 }
	}

}