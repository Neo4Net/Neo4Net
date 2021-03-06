﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{

	using Org.Neo4j.Graphdb;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using PopulationProgress = Org.Neo4j.Storageengine.Api.schema.PopulationProgress;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class RecoveringIndexProxy : AbstractSwallowingIndexProxy
	{
		 internal RecoveringIndexProxy( CapableIndexDescriptor capableIndexDescriptor ) : base( capableIndexDescriptor, null )
		 {
		 }

		 public override InternalIndexState State
		 {
			 get
			 {
				  return InternalIndexState.POPULATING;
			 }
		 }

		 public override bool AwaitStoreScanCompleted( long time, TimeUnit unit )
		 {
			  throw UnsupportedOperation( "Cannot await population on a recovering index." );
		 }

		 public override void Activate()
		 {
			  throw UnsupportedOperation( "Cannot activate recovering index." );
		 }

		 public override void Validate()
		 {
			  throw UnsupportedOperation( "Cannot validate recovering index." );
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  throw UnsupportedOperation( "Unexpected call for validating value while recovering." );
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  throw UnsupportedOperation( "Cannot snapshot a recovering index." );
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  throw UnsupportedOperation( "Cannot get index configuration from recovering index." );
		 }

		 public override void Drop()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public IndexPopulationFailure getPopulationFailure() throws IllegalStateException
		 public override IndexPopulationFailure PopulationFailure
		 {
			 get
			 {
				  throw new System.InvalidOperationException( this + " is recovering" );
			 }
		 }

		 public override PopulationProgress IndexPopulationProgress
		 {
			 get
			 {
				  throw new System.InvalidOperationException( this + " is recovering" );
			 }
		 }

		 private System.NotSupportedException UnsupportedOperation( string message )
		 {
			  return new System.NotSupportedException( message + " Recovering Index" + Descriptor );
		 }
	}

}