/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Neo4Net.Cypher.@internal.v3_5.frontend.phases
{
	public interface CompilationPhaseTracer
	{

		 CompilationPhaseTracer_CompilationPhaseEvent BeginPhase( CompilationPhaseTracer_CompilationPhase phase );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CompilationPhaseTracer NO_TRACING = new CompilationPhaseTracer()
	//	 {
	//		  @@Override public CompilationPhaseEvent beginPhase(CompilationPhase phase)
	//		  {
	//				return NONE_PHASE;
	//		  }
	//	 };
	}

	public static class CompilationPhaseTracer_Fields
	{
		 public static readonly CompilationPhaseEvent NonePhase = () =>
		 {
		 };

	}

	 public enum CompilationPhaseTracer_CompilationPhase
	 {
		  Parsing,
		  DeprecationWarnings,
		  SemanticCheck,
		  AstRewrite,
		  LogicalPlanning,
		  CodeGeneration,
		  PipeBuilding,
	 }

	 public interface CompilationPhaseTracer_CompilationPhaseEvent : AutoCloseable
	 {
		  void Close();
	 }

}