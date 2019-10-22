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
namespace Neo4Net.Tooling.procedure
{
	using AutoService = com.google.auto.service.AutoService;


	using UserFunction = Neo4Net.Procedure.UserFunction;
	using CustomNameExtractor = Neo4Net.Tooling.procedure.compilerutils.CustomNameExtractor;
	using TypeMirrorUtils = Neo4Net.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using Neo4Net.Tooling.procedure.visitors;
	using UserFunctionVisitor = Neo4Net.Tooling.procedure.visitors.UserFunctionVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.procedure.CompilerOptions.IGNORE_CONTEXT_WARNINGS_OPTION;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AutoService(Processor.class) public class UserFunctionProcessor extends DuplicationAwareBaseProcessor<org.Neo4Net.procedure.UserFunction>
	public class UserFunctionProcessor : DuplicationAwareBaseProcessor<UserFunction>
	{

		 private static readonly Type<UserFunction> _supportedAnnotationType = typeof( UserFunction );

		 public UserFunctionProcessor() : base(_supportedAnnotationType, CustomNameExtractor(), processingEnvironment ->
		 {
		  {
				Elements elementUtils = processingEnvironment.ElementUtils;
				Types typeUtils = processingEnvironment.TypeUtils;
				TypeMirrorUtils typeMirrorUtils = new TypeMirrorUtils( typeUtils, elementUtils );

				return new UserFunctionVisitor( new FunctionVisitor<Neo4Net.Procedure.UserFunction>( _supportedAnnotationType, typeUtils, elementUtils, typeMirrorUtils, CustomNameExtractor(), processingEnvironment.Options.containsKey(IGNORE_CONTEXT_WARNINGS_OPTION) ) );
		  });
		 }

		 private static System.Func<UserFunction, Optional<string>> CustomNameExtractor()
		 {
			  return function => CustomNameExtractor.getName( function.name, function.value );
		 }
	}

}