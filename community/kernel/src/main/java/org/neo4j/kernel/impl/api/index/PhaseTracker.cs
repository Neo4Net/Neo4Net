﻿/*
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
	public interface PhaseTracker
	{
		 void EnterPhase( PhaseTracker_Phase phase );

		 void Stop();
	}

	public static class PhaseTracker_Fields
	{
		 public static readonly PhaseTracker NullInstance = new PhaseTracker_NullPhaseTracker();
	}

	 public enum PhaseTracker_Phase
	 {
		  // The order in which the phases are declared defines the order in which they will be printed in the log.
		  // Keep them arranged in the order in which they naturally are seen during index population.
		  Scan,
		  Write,
		  Merge,
		  Build,
		  ApplyExternal,
		  Flip
	 }

	 public class PhaseTracker_NullPhaseTracker : PhaseTracker
	 {
		  public override void EnterPhase( PhaseTracker_Phase phase )
		  {
		  }

		  public override void Stop()
		  {
		  }
	 }

}