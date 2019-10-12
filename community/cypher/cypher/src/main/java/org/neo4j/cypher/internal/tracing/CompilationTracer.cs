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
namespace Org.Neo4j.Cypher.@internal.tracing
{
	using CompilationPhaseTracer = Org.Neo4j.Cypher.@internal.v3_5.frontend.phases.CompilationPhaseTracer;

	public interface CompilationTracer
	{
		 CompilationTracer_QueryCompilationEvent CompileQuery( string query );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CompilationTracer NO_COMPILATION_TRACING = new CompilationTracer()
	//	 {
	//		  @@Override public QueryCompilationEvent compileQuery(String query)
	//		  {
	//				return NONE_EVENT;
	//		  }
	//
	//		  private final QueryCompilationEvent NONE_EVENT = new QueryCompilationEvent()
	//		  {
	//				@@Override public CompilationPhaseEvent beginPhase(CompilationPhase phase)
	//				{
	//					 return NONE_PHASE;
	//				}
	//
	//				@@Override public void close()
	//				{
	//				}
	//		  };
	//	 };
	}

	 public interface CompilationTracer_QueryCompilationEvent : AutoCloseable, CompilationPhaseTracer
	 {
		  void Close();
	 }

}