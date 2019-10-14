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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;

	public abstract class IndexPopulationFailure
	{
		 public abstract string AsString();

		 public abstract IndexPopulationFailedKernelException AsIndexPopulationFailure( SchemaDescriptor descriptor, string indexUserDescriptor );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static IndexPopulationFailure failure(final Throwable failure)
		 public static IndexPopulationFailure Failure( Exception failure )
		 {
			  return new IndexPopulationFailureAnonymousInnerClass( failure );
		 }

		 private class IndexPopulationFailureAnonymousInnerClass : IndexPopulationFailure
		 {
			 private Exception _failure;

			 public IndexPopulationFailureAnonymousInnerClass( Exception failure )
			 {
				 this._failure = failure;
			 }

			 public override string asString()
			 {
				  return Exceptions.stringify( _failure );
			 }

			 public override IndexPopulationFailedKernelException asIndexPopulationFailure( SchemaDescriptor descriptor, string indexUserDescription )
			 {
				  return new IndexPopulationFailedKernelException( indexUserDescription, _failure );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static IndexPopulationFailure failure(final String failure)
		 public static IndexPopulationFailure Failure( string failure )
		 {
			  return new IndexPopulationFailureAnonymousInnerClass2( failure );
		 }

		 private class IndexPopulationFailureAnonymousInnerClass2 : IndexPopulationFailure
		 {
			 private string _failure;

			 public IndexPopulationFailureAnonymousInnerClass2( string failure )
			 {
				 this._failure = failure;
			 }

			 public override string asString()
			 {
				  return _failure;
			 }

			 public override IndexPopulationFailedKernelException asIndexPopulationFailure( SchemaDescriptor descriptor, string indexUserDescription )
			 {
				  return new IndexPopulationFailedKernelException( indexUserDescription, _failure );
			 }
		 }

		 public static string AppendCauseOfFailure( string message, string causeOfFailure )
		 {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("%s: Cause of failure:%n" + "==================%n%s%n==================", message, causeOfFailure);
			  return string.Format( "%s: Cause of failure:%n" + "==================%n%s%n==================", message, causeOfFailure );
		 }
	}

}